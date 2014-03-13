using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    class CrawlStat : TableEntity
    {
        public string counter { get; set; }

        public CrawlStat() { }

        public CrawlStat(string value)
        {
            this.PartitionKey = "numberofurlcrawled";
            this.RowKey = "numberofurlcrawled";

            this.counter = value;
        }
    }
}
