using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    class Result : TableEntity
    {
        public string word { get; set; }
        public string url { get; set; }
        public string date { get; set; }

        public Result() { }

        public Result(string word, string url, string date)
        {
            this.PartitionKey = word;
            this.RowKey = url;

            this.word = word;
            this.url = url;
            this.date = date;
        }
    }
}
