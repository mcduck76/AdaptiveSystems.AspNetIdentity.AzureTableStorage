using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveSoftware.AspNetIdentity.AzureTableStorage
{
    public class UserNameIndex : TableEntity
    {
        public UserNameIndex() { }
        public UserNameIndex(string base64UserName, string userId)
        {
            PartitionKey = base64UserName;
            RowKey = base64UserName;
            UserId = userId;
        }

        public string UserId { get; set; }
    }
}
