using System;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NAL;
using BLL;
using Newtonsoft.Json.Linq;

namespace NeoSQLDB
{
    class Program
    {
        /// <summary>
        /// Timer for getting new blocks on MainNet
        /// </summary>
        private static Timer ReadMainNetTimer = new Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
        /// <summary>
        /// Timer for getting new blocks on TestNet
        /// </summary>
        private static Timer ReadTestNetTimer = new Timer(TimeSpan.FromSeconds(5).TotalMilliseconds);
        /// <summary>
        /// BlockingCollection qeueu for blocks that needs to be synced MainNet.
        /// </summary>
        private static BlockingCollection<int> QueueMainNet = new BlockingCollection<int>();
        /// <summary>
        /// BlockingCollection qeueu for blocks that needs to be synced TestNet
        /// </summary>
        private static BlockingCollection<int> QueueTestNet = new BlockingCollection<int>();
        /// <summary>
        /// Application close variable
        /// </summary>
        private static bool exit = false;
        /// <summary>
        /// Initialize node access layer
        /// </summary>
        private static NodeLayer nodeLayer = new NodeLayer();
        /// <summary>
        /// Initialize business access layer
        /// </summary>
        private static BusinessLayer businessLayer = new BusinessLayer();

        /// <summary>
        /// Main method for syncing blocks.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            //Console.ReadLine();
            if (Settings.Default.DBMainNet.Active)
            {
                //Create ElapsedEventHandler for reading new blocks MainNet
                ReadMainNetTimer.AutoReset = false;
                ReadMainNetTimer.Elapsed += new ElapsedEventHandler(ReadMainNet);
                ReadMainNetTimer.Enabled = Settings.Default.DBMainNet.Active;

                var ConsumerMainNet = Task.Factory.StartNew(() => WriteMainNet());
            }
            if (Settings.Default.DBTestNet.Active)
            {
                //Create ElapsedEventHandler for reading new blocks MainNet
                ReadTestNetTimer.AutoReset = false;
                ReadTestNetTimer.Elapsed += new ElapsedEventHandler(ReadTestNet);
                ReadTestNetTimer.Enabled = Settings.Default.DBTestNet.Active;

                var ConsumerTestNet = Task.Factory.StartNew(() => WriteTestNet());
            }

            Console.WriteLine("Press \'exit\' to quit.");
            string t;
            while (true)
            {
                t = Console.ReadLine();
                if (t == "exit")
                {
                    Console.WriteLine("Exiting ...");
                    exit = true;
                }
            }
        }
        /// <summary>
        /// Main method for getting new blocks from node MainNet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ReadMainNet(object sender, ElapsedEventArgs e)
        {
            try
            {
                //do only if all blocks are processed
                if (QueueMainNet.Count == 0)
                {
                    //Get max block from node
                    JObject joe = null;
                    while (joe == null)
                    {
                        foreach (var node in Settings.Default.NodesMainNet.Nodes)
                        {
                            joe = nodeLayer.InvokeMethod("getblockcount", node, "");
                            if (joe != null)
                            {
                                break;
                            }
                        }
                    }

                    int toblock = int.Parse(joe["result"].ToString());
                    //Get max block from database
                    int maxblock = businessLayer.GetMaxBlockDB(Settings.Default.DBMainNet.Connection);

                    //Add all missing blocks to Queue
                    for (int i = maxblock + 1; i < toblock; i++)
                    {
                        try
                        {
                            QueueMainNet.TryAdd(i);
                        }
                        catch (InvalidOperationException)
                        {
                            Console.WriteLine("MainNet InvalidOperationException");
                            break;
                        }
                    }
                    Console.WriteLine("MainNet Looking for new blocks ...");
                }
            }
            finally
            {
                //Start timer again after adding all new blocks.
                ReadMainNetTimer.Start();
            }
        }
        /// <summary>
        /// Main method for adding new blocks to database MainNet
        /// </summary>
        private static void WriteMainNet()
        {
            try
            {
                try
                {
                    // Runs automatically if a new block is added to the queue
                    foreach (var t in QueueMainNet.GetConsumingEnumerable())
                    {
                        if (exit)
                        {
                            Environment.Exit(0);
                        }

                        JObject joe = null;
                        //get the new block
                        while (joe == null)
                        {
                            foreach (var node in Settings.Default.NodesMainNet.Nodes)
                            {
                                joe = nodeLayer.InvokeMethod("getblock", node, t, 1);
                                if (joe != null)
                                {
                                    break;
                                }
                            }
                        }
                        //write block to database
                        if (businessLayer.SyncBlock(joe, Settings.Default.DBMainNet.Connection, false))
                        {
                            //Console.WriteLine("MainNet Synced:{0} ", t);
                            if ((t % 200) == 0)
                            {
                                Console.WriteLine("MainNet Synced:{0} ", t);
                            }
                        }
                        else
                        {
                            Console.WriteLine("MainNet Error Syncing Block: {0}. Waiting for user ...", t);
                            Console.ReadLine();
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("MainNet InvalidOperationException");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e.StackTrace.ToString());
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine("MainNet Blocks in Queue={0}", QueueMainNet.Count);
            }
        }
        /// <summary>
        /// Main method for getting new blocks from node TestNet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void ReadTestNet(object sender, ElapsedEventArgs e)
        {
            try
            {
                //do only if all blocks are processed
                if (QueueTestNet.Count == 0)
                {
                    //Get max block from node
                    JObject joe = null;
                    while (joe == null)
                    {
                        foreach (var node in Settings.Default.NodesTestNet.Nodes)
                        {
                            joe = nodeLayer.InvokeMethod("getblockcount", node, "");
                            if (joe != null)
                            {
                                break;
                            }
                        }
                    }
                    int toblock = int.Parse(joe["result"].ToString());
                    //Get max block from database
                    int maxblock = businessLayer.GetMaxBlockDB(Settings.Default.DBTestNet.Connection);

                    //Add all missing blocks to Queue
                    for (int i = maxblock + 1; i < toblock; i++)
                    {
                        try
                        {
                            QueueTestNet.TryAdd(i);
                        }
                        catch (InvalidOperationException)
                        {
                            Console.WriteLine("TestNet InvalidOperationException");
                            break;
                        }
                    }
                    Console.WriteLine("TestNet Looking for new blocks ...");
                }
            }
            finally
            {
                //Start timer again after adding all new blocks.
                ReadTestNetTimer.Start();
            }
        }
        /// <summary>
        /// Main method for adding new blocks to database MainNet
        /// </summary>
        private static void WriteTestNet()
        {
            try
            {
                try
                {
                    // Runs automatically if a new block is added to the queue
                    foreach (var t in QueueTestNet.GetConsumingEnumerable())
                    {
                        if (exit)
                        {
                            Environment.Exit(0);
                        }

                        JObject joe = null;
                        //get the new block
                        while (joe == null)
                        {
                            foreach (var node in Settings.Default.NodesTestNet.Nodes)
                            {
                                joe = nodeLayer.InvokeMethod("getblock", node, t, 1);
                                if (joe != null)
                                {
                                    break;
                                }
                            }
                        }
                        //write block to database
                        if (businessLayer.SyncBlock(joe, Settings.Default.DBTestNet.Connection, false))
                        {
                            Console.WriteLine("TestNet Synced:{0} ", t);
                            if ((t % 200) == 0)
                            {
                                Console.WriteLine("TestNet Synced:{0} ", t);
                            }
                        }
                        else
                        {
                            Console.WriteLine("TestNet Error Syncing Block: {0}. Waiting for user ...", t);
                            Console.ReadLine();
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("TestNet InvalidOperationException");
                    Console.ReadLine();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e.StackTrace.ToString());
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine("TestNet Blocks in Queue={0}", QueueTestNet.Count);
            }
        }
    }
}
