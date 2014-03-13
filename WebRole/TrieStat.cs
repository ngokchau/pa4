using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole
{
    public class TrieStat : TableEntity
    {
        public string size { get; set; }
        public string last { get; set; }

        public TrieStat(string type, string line, string size)
        {
            this.PartitionKey = type;
            this.RowKey = type;

            this.size = size;
            this.last = line;
        }

        public TrieStat() { }
    }
}