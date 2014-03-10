using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole
{
    public class Stats : TableEntity
    {
        public string wordCounter { get; set; }

        public Stats(string type, string value)
        {
            this.PartitionKey = type;
            this.RowKey = type;

            this.wordCounter = value;
        }

        public Stats() { }
    }
}