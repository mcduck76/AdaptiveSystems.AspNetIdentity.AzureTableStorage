using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSoftware.AspNetIdentity.AzureTableStorage
{
    public class EmailIndex : TableEntity
    {
        public EmailIndex() { }
        public EmailIndex(string base64Email, string userId)
        {
            PartitionKey = base64Email;
            RowKey = base64Email;
            UserId = userId;
        }

        public string UserId { get; set; }
    }
}
