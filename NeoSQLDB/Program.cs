using System;
using System.Timers;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NAL;
using BLL;
using Newtonsoft.Json.Linq;
using System.IO;
using Newtonsoft.Json;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

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
        private static BlockingCollection<long> QueueMainNet = new BlockingCollection<long>();
        private static BlockingCollection<long> QueueNEPMainNet = new BlockingCollection<long>();
        /// <summary>
        /// BlockingCollection qeueu for blocks that needs to be synced TestNet
        /// </summary>
        private static BlockingCollection<long> QueueTestNet = new BlockingCollection<long>();
        private static BlockingCollection<long> QueueNEPTestNet = new BlockingCollection<long>();
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
            //List<string> localNode = new List<string>(new string[] { Settings.Default.NodesMainNet.Nodes.First().ToString() });
            //JObject joe = nodeLayer.Invoke("getblocknotification", localNode, 1829894, 1);

            //businessLayer.SyncNEP5(joe, Settings.Default.AddressVersion, Settings.Default.NodesMainNet.Nodes, 1829894, Settings.Default.DBMainNet.Connection, false);

            Console.ReadLine();
            //Console.ReadLine();
            if (Settings.Default.DBMainNet.Active)
            {
                //Create ElapsedEventHandler for reading new blocks MainNet
                ReadMainNetTimer.AutoReset = false;
                ReadMainNetTimer.Elapsed += new ElapsedEventHandler(ReadMainNet);
                ReadMainNetTimer.Enabled = Settings.Default.DBMainNet.Active;

                var ConsumerMainNet = Task.Factory.StartNew(() => WriteMainNet());

                if(Settings.Default.DBMainNet.NepSyncActive)
                {
                    var ConsumerNEPMainNet = Task.Factory.StartNew(() => WriteNEPMainNet());
                }
            }
            if (Settings.Default.DBTestNet.Active)
            {
                //Create ElapsedEventHandler for reading new blocks MainNet
                ReadTestNetTimer.AutoReset = false;
                ReadTestNetTimer.Elapsed += new ElapsedEventHandler(ReadTestNet);
                ReadTestNetTimer.Enabled = Settings.Default.DBTestNet.Active;

                var ConsumerTestNet = Task.Factory.StartNew(() => WriteTestNet());
                if (Settings.Default.DBTestNet.NepSyncActive)
                {
                    var ConsumerNEPTestnNet = Task.Factory.StartNew(() => WriteNEPTestNet());
                }
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
                    JObject joe = nodeLayer.Invoke("getblockcount", Settings.Default.NodesMainNet.Nodes, "");

                    long toblock = long.Parse(joe["result"].ToString());
                    //Get max block from database
                    long maxblock = businessLayer.GetMaxBlockDB(Settings.Default.DBMainNet.Connection);

                    //Add all missing blocks to Queue
                    for (long i = maxblock + 1; i < toblock; i++)
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
                if (Settings.Default.DBMainNet.NepSyncActive)
                {
                    //check nep queue
                    if (QueueNEPMainNet.Count == 0)
                    {
                        //Get max block from node
                        JObject joe = nodeLayer.Invoke("getblockcount", Settings.Default.NodesMainNet.Nodes, "");
                        long toblock = businessLayer.GetMaxBlockDB(Settings.Default.DBMainNet.Connection);
                        long maxblock = businessLayer.GetMaxBlockNEPDB(Settings.Default.DBMainNet.Connection);
                        //Add all missing blocks to Queue
                        for (long i = maxblock + 1; i < toblock; i++)
                        {
                            try
                            {
                                QueueNEPMainNet.TryAdd(i);
                            }
                            catch (InvalidOperationException)
                            {
                                Console.WriteLine("MainNet NEP InvalidOperationException");
                                break;
                            }
                        }
                        Console.WriteLine("MainNet NEP Looking for new blocks ...");
                    }
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

                        JObject joe = nodeLayer.Invoke("getblock", Settings.Default.NodesMainNet.Nodes, t, 1);
                        //get the new block

                        //write block to database
                        if (businessLayer.SyncBlock(joe, Settings.Default.DBMainNet.Connection, Settings.Default.DBMainNet.Debug))
                        {
                            /*
                            if(result.Item2)
                            {
                                //sync nep5 todo or add to blockingqueee?
                                try
                                {
                                    QueueNEPMainNet.TryAdd(t);
                                }
                                catch (InvalidOperationException)
                                {
                                    Console.WriteLine("MainNet NEP InvalidOperationException");
                                    break;
                                }
                            }
                            */
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
                    JObject joe = nodeLayer.Invoke("getblockcount", Settings.Default.NodesMainNet.Nodes, "");

                    long toblock = long.Parse(joe["result"].ToString());
                    //Get max block from database
                    long maxblock = businessLayer.GetMaxBlockDB(Settings.Default.DBTestNet.Connection);

                    //Add all missing blocks to Queue
                    for (long i = maxblock + 1; i < toblock; i++)
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
                if (Settings.Default.DBTestNet.NepSyncActive)
                {
                    //check nep queue
                    if (QueueNEPTestNet.Count == 0)
                    {
                        //Get max block from node
                        JObject joe = nodeLayer.Invoke("getblockcount", Settings.Default.NodesTestNet.Nodes, "");
                        long toblock = businessLayer.GetMaxBlockDB(Settings.Default.DBTestNet.Connection);
                        long maxblock = businessLayer.GetMaxBlockNEPDB(Settings.Default.DBTestNet.Connection);
                        //Add all missing blocks to Queue
                        for (long i = maxblock + 1; i < toblock; i++)
                        {
                            try
                            {
                                QueueNEPTestNet.TryAdd(i);
                            }
                            catch (InvalidOperationException)
                            {
                                Console.WriteLine("TestNet NEP InvalidOperationException");
                                break;
                            }
                        }
                        Console.WriteLine("TestNet NEP Looking for new blocks ...");
                    }
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

                        JObject joe = nodeLayer.Invoke("getblock", Settings.Default.NodesMainNet.Nodes,t, 1);
                        //get the new block

                        //write block to database
                        if (businessLayer.SyncBlock(joe, Settings.Default.DBTestNet.Connection, Settings.Default.DBTestNet.Debug))
                        {
                            /*
                            if(result.Item2)
                            {
                                try
                                {
                                    QueueNEPTestNet.TryAdd(t);
                                }
                                catch (InvalidOperationException)
                                {
                                    Console.WriteLine("MainNet NEP InvalidOperationException");
                                    break;
                                }
                            }*/
                            //Console.WriteLine("TestNet Synced:{0} ", t);
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
        private static void WriteNEPMainNet()
        {
            try
            {
                try
                {
                    // Runs automatically if a new block is added to the queue
                    foreach (var t in QueueNEPMainNet.GetConsumingEnumerable())
                    {
                        JObject joe2 = nodeLayer.Invoke("getblocknotification", Settings.Default.NodesNEPMainNet.Nodes, t, 1);

                        if (joe2["error"] == null)
                        {
                            if (businessLayer.SyncNEP5(joe2, Settings.Default.AddressVersion, Settings.Default.NodesNEPMainNet.Nodes, t, Settings.Default.DBMainNet.Connection, Settings.Default.DBMainNet.Debug))
                            {
                                if ((t % 200) == 0)
                                {
                                    Console.WriteLine("MainNet NEP Synced:{0} ", t);
                                }
                            }
                            else
                            {
                                Console.WriteLine("FAILURE or no TRANSFER MainNet NEP Synced:{0} ", t);
                            }
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("MainNet NEP InvalidOperationException");
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
                Console.WriteLine("MainNet NEP Blocks in Queue={0}", QueueNEPMainNet.Count);
            }
        }
        private static void WriteNEPTestNet()
        {
            try
            {
                try
                {
                    // Runs automatically if a new block is added to the queue
                    foreach (var t in QueueNEPTestNet.GetConsumingEnumerable())
                    {
                        JObject joe2 = nodeLayer.Invoke("getblocknotification", Settings.Default.NodesNEPTestNet.Nodes, t, 1);

                        if (businessLayer.SyncNEP5(joe2, Settings.Default.AddressVersion, Settings.Default.NodesNEPTestNet.Nodes, t, Settings.Default.DBTestNet.Connection, Settings.Default.DBTestNet.Debug))
                        {
                            if ((t % 200) == 0)
                            {
                                Console.WriteLine("TestNet NEP Synced:{0} ", t);
                            }
                        }
                        else
                        {
                            Console.WriteLine("FAILURE or no Transfer TestNet NEP Synced:{0} ", t);
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("TestNet NEP InvalidOperationException");
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
                Console.WriteLine("TestNet NEP Blocks in Queue={0}", QueueNEPTestNet.Count);
            }
        }
    }
}
