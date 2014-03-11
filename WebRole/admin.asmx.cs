using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;
using WorkerRole;

namespace WebRole
{
    /// <summary>
    /// Summary description for admin
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    [ScriptService]
    public class admin : System.Web.Services.WebService
    {
        private static Trie trie;
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
        private static CloudQueue cmdQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlercmd");
        private static CloudQueue urlQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlerurl");
        private static CloudQueue errorQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlererror");
        private static CloudQueue lastTenUrlQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("lasttenurlcrawled");
        private static CloudTable index = storageAccount.CreateCloudTableClient().GetTableReference("krawlerindex");

        [WebMethod]
        public void BuildTrie()
        {
            trie = new Trie();
            CloudBlockBlob wikidata = storageAccount.CreateCloudBlobClient().GetContainerReference("querysuggestion").GetBlockBlobReference("clean-wiki-full.txt");
            StreamReader sr = new StreamReader(wikidata.OpenRead());

            int counter = 1;
            while (!sr.EndOfStream && (counter % 50000 != 0 || Convert.ToInt32(GetPerformance("Memory", "Available MBytes", "")) > 70))
            {
                trie.Insert(sr.ReadLine());
                counter++;
            }
        }

        [WebMethod]
        public List<string> Suggest(string input)
        {
            if (trie == null)
            {
                BuildTrie();
            }
            trie.Search(input);
            return trie.Suggestions;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public List<string> Search(string input)
        {
            TableQuery<Result> query = new TableQuery<Result>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, input));
            List<string> results = new List<string>();
            
            foreach (Result result in index.ExecuteQuery(query))
            {
                results.Add(HttpUtility.UrlDecode(result.url));
            }

            return results;
        }

        //[WebMethod]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public string GetNumbUrlCrawled()
        //{
        //    statTable.CreateIfNotExists();
        //    TableQuery<Stat> query = new TableQuery<Stat>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "numburlcrawled"));

        //    string result = "0";
        //    foreach (Stat stat in statTable.ExecuteQuery(query))
        //    {
        //        result = stat.counter;
        //    }

        //    return new JavaScriptSerializer().Serialize(result);
        //}

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetPerformance(string categoryName, string counterName, string instanceName)
        {
            PerformanceCounter pc = new PerformanceCounter(categoryName, counterName, instanceName);
            if (categoryName == "Processor")
            {
                pc.NextValue();
                Thread.Sleep(1000);
            }
            return new JavaScriptSerializer().Serialize(pc.NextValue());
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetStateOfWorker()
        {
            try { 
                return new JavaScriptSerializer().Serialize((cmdQueue.PeekMessage() != null) ? cmdQueue.PeekMessage().AsString : "stop");
            }
            catch (Exception e)
            {
                errorQueue.FetchAttributes();
                if (errorQueue.ApproximateMessageCount >= 10)
                {
                    errorQueue.DeleteMessage(errorQueue.GetMessage());
                }
                errorQueue.AddMessage(new CloudQueueMessage(e.Message + " " + DateTime.UtcNow));
                return new JavaScriptSerializer().Serialize("stop");
            }
        }

        [WebMethod]
        public void AddCommand(string cmd)
        {
            cmdQueue.CreateIfNotExists();
            cmdQueue.Clear();
            cmdQueue.AddMessage(new CloudQueueMessage(cmd));
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetUrlQueueSize()
        {
            urlQueue.FetchAttributesAsync();
            return new JavaScriptSerializer().Serialize(urlQueue.ApproximateMessageCount);
        }

        [WebMethod]
        public void ClearUrlQueue()
        {
            urlQueue.Clear();
        }

        [WebMethod]
        public List<string> GetLastTen(string type)
        {
            IEnumerable<CloudQueueMessage> msgs = (type == "errors") ? errorQueue.PeekMessages(10) : lastTenUrlQueue.PeekMessages(10);
            List<string> results = new List<string>();

            foreach (CloudQueueMessage msg in msgs)
            {
                results.Add(msg.AsString);
            }

            return results;
        }

        [WebMethod]
        public void AddError()
        {
            errorQueue.FetchAttributes();
            if (errorQueue.ApproximateMessageCount >= 10)
            {
                errorQueue.DeleteMessage(errorQueue.GetMessage());
            }
            errorQueue.AddMessage(new CloudQueueMessage("Error " + DateTime.UtcNow.ToString()));
        }

        [WebMethod]
        public void ClearErrors()
        {
            errorQueue.Clear();
        }
    }
}
