using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRole
{
    public class Stat : TableEntity
    {
        public string counter { get; set; }

        public Stat(string type, string value)
        {
            this.PartitionKey = type;
            this.RowKey = type;

            this.counter = value;
        }

        public Stat() { }
    }
}