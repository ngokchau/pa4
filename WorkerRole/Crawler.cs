using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace WorkerRole
{
    class Crawler
    {
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
        private static CloudQueue urlQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlerurl");
        private static CloudTable index = storageAccount.CreateCloudTableClient().GetTableReference("krawlerindex");
        private static CloudQueue errorQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlererror");

        public Crawler() { }

        public void ParseSitemap(string xml)
        {
            XmlTextReader xmlReader = new XmlTextReader(WebRequest.Create(xml).GetResponse().GetResponseStream());
            string lastThreeMonthsXmlRegex = @"^(http|https):\/\/[a-zA-Z0-9\-\.]+\.cnn\.com\/[a-zA-Z0-9\/\-]+2014-(02|03|01)(\.xml)$";
            string htmlRegex = @"^(http|https):\/\/[a-zA-Z0-9\-\.]+\.cnn\.com\/[a-zA-Z\d\/\.\-]+\/[a-zA-Z\d\-]+(\.cnn\.html|\.html|\.wtvr\.html|[a-zA-Z\d]+|\?[a-zA-Z\=a-zA-Z\&+\=a-zA-z0-9]+)$";

            try
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Text)
                    {
                        if (Regex.Match(xmlReader.Value, htmlRegex).Success)
                        {
                            urlQueue.AddMessage(new CloudQueueMessage(xmlReader.Value));
                        }
                        else if (Regex.Match(xmlReader.Value, lastThreeMonthsXmlRegex).Success)
                        {
                            ParseSitemap(xmlReader.Value);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                errorQueue.FetchAttributes();
                if (errorQueue.ApproximateMessageCount >= 3)
                {
                    errorQueue.DeleteMessage(errorQueue.GetMessage());
                }
                errorQueue.AddMessage(new CloudQueueMessage(e.Message + " " + DateTime.UtcNow));
            }
        }
    }
}
