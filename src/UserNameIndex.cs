using Microsoft.WindowsAzure.Storage.Table;
using NExtensions;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class UserNameIndex : TableEntity
    {
        public UserNameIndex() { }
        public UserNameIndex(string userName) : this(userName, string.Empty) { }
        public UserNameIndex(string userName, string userId)
        {
            PartitionKey = userName.Base64Encode();
            RowKey = PartitionKey;
            UserId = userId;
        }

        public string UserId { get; set; }
    }
}
