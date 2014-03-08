using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerRole
{
    class Error : TableEntity
    {
        public Error() { }

        public Error(string error, DateTime dt)
        {
            this.PartitionKey = "error";
            this.RowKey = error;
            this.Timestamp = dt;
        }
    }
}
