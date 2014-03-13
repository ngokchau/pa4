using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using HtmlAgilityPack;
using System.Web;

namespace WorkerRole
{
    class Crawler
    {
        private static CloudStorageAccount storageAccount;
        private static CloudQueue urlQueue;
        private static CloudQueue cmdQueue;
        private static CloudTable index;
        private static CloudQueue errorQueue;
        private static CloudQueue lastTenUrlQueue;
        private static CloudTable crawlStatTable;
        private static int crawlCounter;
        private static List<string> disallowedPaths;
        private static HashSet<string> visited;

        public Crawler()
        {
            storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            urlQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlerurl");
            cmdQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlercmd");
            index = storageAccount.CreateCloudTableClient().GetTableReference("krawlerindex");
            errorQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("krawlererror");
            lastTenUrlQueue = storageAccount.CreateCloudQueueClient().GetQueueReference("lasttenurlcrawled");
            crawlStatTable = storageAccount.CreateCloudTableClient().GetTableReference("krawlerstattable");
            disallowedPaths = new List<string>();
            visited = new HashSet<string>();

            crawlStatTable.CreateIfNotExists();
            crawlCounter = (NumberOfUrlCrawled() != 0) ? NumberOfUrlCrawled() : 0;
        }

        public void Crawl(string url)
        {
            if (IsAllowedPath(url))
            {
                try
                {
                    string htmlRegex = @"^(http|https):\/\/[a-zA-Z0-9\-\.]+\.cnn\.com\/([a-zA-Z\d\/\.\-]+|\.cnn\.html|\.html|\.wtvr\.html|[a-zA-Z\d]+\?[a-zA-Z\=a-zA-Z\&+\=a-zA-z0-9]+|)$";
                    HtmlDocument htmlDoc = new HtmlDocument();

                    htmlDoc.Load(WebRequest.Create(url).GetResponse().GetResponseStream());
                    string title = (htmlDoc.DocumentNode.SelectSingleNode("//head/title") != null) ? htmlDoc.DocumentNode.SelectSingleNode("//head/title").InnerHtml : "";
                    string date = (htmlDoc.DocumentNode.SelectSingleNode("//head/meta[@http-equiv='last-modified']") != null) ? htmlDoc.DocumentNode.SelectSingleNode("//head/meta[@http-equiv='last-modified']").Attributes["content"].Value : "";

                    string[] words = title.Split(' ', ',', ':', '-', '\t', '&');
                    index.CreateIfNotExists();

                    foreach (string word in words)
                    {
                        TableOperation insertRecord = TableOperation.InsertOrReplace(new Result(word.ToLower(), HttpUtility.UrlEncode(url), date));
                        index.Execute(insertRecord);
                    }

                    Uri uri = new Uri(url);
                    HtmlNodeCollection links = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
                    foreach (HtmlNode link in links)
                    {
                        string scheme = new Uri(uri, link.Attributes["href"].Value).Scheme.ToString();
                        string host = new Uri(uri, link.Attributes["href"].Value).Host.ToString();
                        string path = new Uri(uri, link.Attributes["href"].Value).PathAndQuery.ToString();
                        string newUrl = scheme + "://" + host + path;
                        
                        if (!visited.Contains(newUrl))
                        {
                            if (Regex.Match(newUrl, htmlRegex).Success)
                            {
                                urlQueue.AddMessage(new CloudQueueMessage(newUrl));
                            }
                            visited.Add(newUrl);
                        }
                    }

                    crawlCounter += 1;
                    TableOperation insertCrawlStat = TableOperation.InsertOrReplace(new CrawlStat(crawlCounter.ToString()));
                    crawlStatTable.Execute(insertCrawlStat);

                    LastUrlCrawled(url);
                }
                catch (Exception e)
                {
                    Error(e);
                }
            }
        }

        private int NumberOfUrlCrawled()
        {
            TableQuery<CrawlStat> query = new TableQuery<CrawlStat>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "numberofurlcrawled"));
            string result = "0";

            foreach (CrawlStat stat in crawlStatTable.ExecuteQuery(query))
            {
                result = stat.counter;
            }

            return Convert.ToInt32(result);
        }

        private void Error(Exception error)
        {
            errorQueue.FetchAttributes();
            if (errorQueue.ApproximateMessageCount >= 10)
            {
                errorQueue.DeleteMessage(errorQueue.GetMessage());
            }
            errorQueue.AddMessage(new CloudQueueMessage(error.Message + " " + DateTime.UtcNow));
        }

        private void LastUrlCrawled(string url)
        {
            lastTenUrlQueue.FetchAttributes();
            if (lastTenUrlQueue.ApproximateMessageCount >= 10)
            {
                lastTenUrlQueue.DeleteMessage(lastTenUrlQueue.GetMessage());
            }
            lastTenUrlQueue.AddMessage(new CloudQueueMessage(url));
        }

        private bool IsAllowedPath(string url)
        {
            foreach (string path in disallowedPaths)
            {
                if (url.Contains(path))
                {
                    return false;
                }
            }
            return true;
        }

        public void LoadQueue(string site)
        {
            string xmlDocs = @"^(http|https):\/\/[a-zA-Z0-9\-\.]+\.cnn\.com\/[a-zA-Z0-9\/\-]+(\.xml)$";
            WebRequest request = WebRequest.Create(site + "/robots.txt");
            StreamReader sr = new StreamReader(request.GetResponse().GetResponseStream());

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (Regex.Match(line.Substring(9), xmlDocs).Success)
                {
                    ParseSitemap(line.Substring(9));
                }

                if (line.StartsWith("Disallow: "))
                {
                    disallowedPaths.Add(line.Substring(10));
                }
            }
            cmdQueue.Clear();
            cmdQueue.AddMessage(new CloudQueueMessage("stop"));
        }

        private void ParseSitemap(string xml)
        {
            XmlTextReader xmlReader = new XmlTextReader(WebRequest.Create(xml).GetResponse().GetResponseStream());
            string lastThreeMonthsXmlRegex = @"^(http|https):\/\/[a-zA-Z0-9\-\.]+\.cnn\.com\/[a-zA-Z0-9\/\-]+2014-(03|02|01)(\.xml)$";
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
                Error(e);
            }
        }
    }
}
