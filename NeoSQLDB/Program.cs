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
        private static System.Timers.Timer read = new System.Timers.Timer(TimeSpan.FromSeconds(15).TotalMilliseconds);
        private static BlockingCollection<int> queue = new BlockingCollection<int>();
        private static bool exit = false;
        private static NodeLayer nodeLayer = new NodeLayer();
        private static BusinessLayer businessLayer = new BusinessLayer();

        static void Main(string[] args)
        {
           // JObject joe = nodeLayer.InvokeMethod("getblock", 1774137, 1);
            //if (businessLayer.SyncBlock(joe,false))
            // {
            //    Console.WriteLine("te");
            //}
            Console.ReadLine();
            read.AutoReset = false;
            read.Elapsed += new ElapsedEventHandler(read_block);
            read.Enabled = true;

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

        private static void read_block(object sender, ElapsedEventArgs e)
        {
            try
            {
                //do onlyif all blocks are processed
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
                read.Start();
            }
        }
        private static void RunConsumer()
        {
            try
            {
                try
                {
                    foreach (var t in queue.GetConsumingEnumerable())
                    {
                        if (exit)
                        {
                            Environment.Exit(0);
                        }

                        JObject joe = null;
                        while (joe == null)
                        {
                            joe = nodeLayer.InvokeMethod("getblock", t, 1);
                        }
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
