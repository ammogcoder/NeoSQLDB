using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using NAL;
using BLL;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NeoSQLDB
{
    class Program
    {
        /// <summary>
        /// Timer for getting new blocks
        /// </summary>
        private static System.Timers.Timer read = new System.Timers.Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
        /// <summary>
        /// BlockingCollection qeueu for blocks that needs to be synced.
        /// </summary>
        private static BlockingCollection<int> queue = new BlockingCollection<int>();
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
            Console.ReadLine();

            //Create ElapsedEventHandler for reading new blocks
            read.AutoReset = false;
            read.Elapsed += new ElapsedEventHandler(read_block);
            read.Enabled = true;

            //Create task for syncing blocks
            var consumerWorker = Task.Factory.StartNew(() => RunConsumer());

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
        /// Main method for getting new blocks from node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void read_block(object sender, ElapsedEventArgs e)
        {
            try
            {
                //do only if all blocks are processed
                if (queue.Count == 0)
                {
                    //Get max block from node
                    JObject joe = nodeLayer.InvokeMethod("getblockcount", "");
                    int toblock = int.Parse(joe["result"].ToString());

                    //Get max block from database
                    int maxblock = businessLayer.GetMaxBlockDB();

                    //Add all missing blocks to Queue
                    for (int i = maxblock + 1; i < toblock; i++)
                    {
                        try
                        {
                            queue.TryAdd(i);
                        }
                        catch (InvalidOperationException)
                        {
                            Console.WriteLine("InvalidOperationException");
                            break;
                        }
                    }
                    Console.WriteLine("Looking for new blocks ...");
                }
            }
            finally
            {
                //Start timer again after adding all new blocks.
                read.Start();
            }
        }
        /// <summary>
        /// Main method for adding new blocks to database
        /// </summary>
        private static void RunConsumer()
        {
            try
            {
                try
                {
                    // Runs automatically if a new block is added to the queue
                    foreach (var t in queue.GetConsumingEnumerable())
                    {
                        if (exit)
                        {
                            Environment.Exit(0);
                        }

                        JObject joe = null;
                        //get the new block
                        while (joe == null)
                        {
                            joe = nodeLayer.InvokeMethod("getblock", t, 1);
                        }
                        //write block to database
                        if (businessLayer.SyncBlock(joe, false))
                        {
                            Console.WriteLine("Synced:{0} ", t);
                            if ((t % 500) == 0)
                            {
                                Console.WriteLine("Synced:{0} ", t);
                            }
                        }
                        else
                        {
                            Console.WriteLine("Error Syncing Block: {0}. Waiting for user ...", t);
                            Console.ReadLine();
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.WriteLine("InvalidOperationException");
                    Console.ReadLine();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message, e.StackTrace.ToString());
                Console.ReadLine();
            }
            finally
            {
                Console.WriteLine("Blocks in Queue={0}", queue.Count);
            }
        }
    }
}
