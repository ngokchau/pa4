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
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
        private static CloudQueue cmdQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlercmd");
        private static CloudQueue urlQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlerurl");
        private static CloudQueue errorQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlererror");

        public override void Run()
        {
            // This is a sample worker implementation. Replace with your logic.
            Trace.TraceInformation("WorkerRole entry point called", "Information");

            cmdQueue.CreateIfNotExists();
            urlQueue.CreateIfNotExists();
            errorQueue.CreateIfNotExists();

            Crawler crawler = new Crawler("http://www.cnn.com");

            while (true)
            {
                Thread.Sleep(1000);
                Trace.TraceInformation("Working", "Information");

                CloudQueueMessage cmd = cmdQueue.PeekMessage();
                if (cmd == null || cmd.AsString == "stop")
                {
                    continue;
                }
                else
                {

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
