using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;
using System.Web;

namespace WorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        private static CloudStorageAccount storageAccount;
        private static CloudQueue cmdQueue;
        private static CloudQueue urlQueue;
        private static CloudQueue errorQueue;
        private static CloudQueue lastTenUrlQueue;
        private static CloudTable index;
        private static HashSet<string> visisted;

        public override void Run()
        {
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            cmdQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlercmd");
            urlQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlerurl");
            errorQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlererror");
            lastTenUrlQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("lasttenurlcrawled");
            index = storageAccount.CreateCloudTableClient().GetTableReference("krawlerindex");
            visisted = new HashSet<string>();

            cmdQueue.CreateIfNotExists();
            urlQueue.CreateIfNotExists();
            errorQueue.CreateIfNotExists();
            lastTenUrlQueue.CreateIfNotExists();

            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerRole entry point called", "Information");

            //Crawler crawler = new Crawler();
            //cmdQueue.Clear();
            //cmdQueue.AddMessage(new CloudQueueMessage("stop"));

            while (true)
            {
                Thread.Sleep(500);
                Trace.TraceInformation("Working", "Information");

                CloudQueueMessage cmd = cmdQueue.PeekMessage();
                if (cmd == null || cmd.AsString == "stop")
                {
                    continue;
                }
                //else if (cmd.AsString == "start")
                //{
                //    CloudQueueMessage url = urlQueue.GetMessage();
                //    if (!visisted.Contains(url.AsString))
                //    {
                //        crawler.Crawl(url.AsString);
                //        visisted.Add(url.AsString);
                //    }

                //    urlQueue.DeleteMessage(url);
                //}
                //else if(cmd.AsString == "load" && urlQueue.PeekMessage() == null)
                //{
                //    crawler.LoadQueue("http://www.cnn.com");
                //}
                else
                {
                    continue;
                }
            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }
    }
}
