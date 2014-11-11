using Microsoft.WindowsAzure.Storage.Table;
using NExtensions;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class UserEmailIndex : TableEntity
    {
        public UserEmailIndex() { }
        public UserEmailIndex(string email) : this(email, string.Empty) { }
        public UserEmailIndex(string email, string userId)
        {
            PartitionKey = email.Base64Encode();
            RowKey = PartitionKey;
            UserId = userId;
        }

        public string UserId { get; set; }
    }
}
