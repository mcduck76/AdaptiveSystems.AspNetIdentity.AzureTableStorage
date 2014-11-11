using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class IdentityTables
    {
        private readonly CloudTable usersTable;
        private readonly CloudTable userNamesIndexTable;
        private readonly CloudTable userEmailsIndexTable;
        private readonly CloudTable userExternalLoginsIndexTable;

        public IdentityTables(CloudStorageAccount storageAccount, bool createIfNotExists, string usersTableName, string userNamesIndexTableName, string userEmailsIndexTableName, string userExternalLoginsIndexTableName)
        {
            var tableClient = storageAccount.CreateCloudTableClient();

            usersTable = tableClient.GetTableReference(usersTableName);
            userNamesIndexTable = tableClient.GetTableReference(userNamesIndexTableName);
            userEmailsIndexTable = tableClient.GetTableReference(userEmailsIndexTableName);
            userExternalLoginsIndexTable = tableClient.GetTableReference(userExternalLoginsIndexTableName);

            if (createIfNotExists)
            {
                usersTable.CreateIfNotExists();
                userNamesIndexTable.CreateIfNotExists();
                userEmailsIndexTable.CreateIfNotExists();
                userExternalLoginsIndexTable.CreateIfNotExists();
            }
        }

        public Task<TableResult> InsertUserNamesIndexTableEntity(UserNameIndex entity)
        {
            return InsertAsync(userNamesIndexTable, entity);
        }

        public Task<TableResult> InsertUserEmailsIndexTableEntity(UserEmailIndex entity)
        {
            return InsertAsync(userEmailsIndexTable, entity);
        }

        public Task<TableResult> InsertUserExternalLoginIndexTableEntity(UserExternalLoginIndex entity)
        {
            return InsertAsync(userExternalLoginsIndexTable, entity);
        }

        public Task<TableResult> InsertOrReplaceUserTableEntity(User entity)
        {
            return InsertOrReplaceAsync(usersTable, entity);
        }

        public Task<TableResult> DeleteUserTableEntity(User entity)
        {
            return DeleteAsync(usersTable, entity);
        }

        public Task<TableResult> DeleteUserNamesIndexTableEntity(UserNameIndex entity)
        {
            return DeleteAsync(userNamesIndexTable, entity);
        }

        public Task<TableResult> DeleteUserEmailsIndexTableEntity(UserEmailIndex entity)
        {
            return DeleteAsync(userEmailsIndexTable, entity);
        }

        public Task<TableResult> DeleteUserExternalLoginIndexTableEntity(UserExternalLoginIndex entity)
        {
            return DeleteAsync(userExternalLoginsIndexTable, entity);
        }

        public Task<TableResult> UpdateUserTableEntity(User entity)
        {
            return ReplaceAsync(usersTable, entity);
        }

        public async Task<UserEmailIndex> RetrieveUserEmailsIndexAsync(UserEmailIndex entity)
        {
            return await RetrieveAsync(userEmailsIndexTable, entity);
        }

        public async Task<UserNameIndex> RetrieveUserNamesIndexAsync(UserNameIndex entity)
        {
            return await RetrieveAsync(userNamesIndexTable, entity);
        }

        public async Task<UserExternalLoginIndex> RetrieveUserExternalLoginIndexAsync(UserExternalLoginIndex entity)
        {
            return await RetrieveAsync(userExternalLoginsIndexTable, entity);
        }

        public async Task<User> RetrieveUserAsync(string userId)
        {
            return await RetrieveAsync<User>(usersTable, userId, userId);
        }

        private async Task<T> RetrieveAsync<T>(CloudTable table, T entity) where T : ITableEntity
        {
            return await RetrieveAsync<T>(table, entity.PartitionKey, entity.RowKey);
        }

        private async Task<T> RetrieveAsync<T>(CloudTable table, string partitionKey, string rowKey) where T : ITableEntity
        {
            var retrieveOperation = TableOperation.Retrieve<T>(partitionKey, rowKey);
            var result = await table.ExecuteAsync(retrieveOperation);
            return (T)result.Result;
        }

        private Task<TableResult> InsertAsync(CloudTable table, ITableEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            return table.ExecuteAsync(insertOperation);
        }

        private Task<TableResult> InsertOrReplaceAsync(CloudTable table, ITableEntity entity)
        {
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);
            return table.ExecuteAsync(insertOrReplaceOperation);
        }

        private Task<TableResult> DeleteAsync(CloudTable table, ITableEntity entity)
        {
            entity.ETag = "*";
            var deleteOperation = TableOperation.Delete(entity);
            return table.ExecuteAsync(deleteOperation);
        }

        private Task<TableResult> ReplaceAsync(CloudTable table, ITableEntity entity)
        {
            entity.ETag = "*";
            var replaceOperation = TableOperation.Replace(entity);
            return table.ExecuteAsync(replaceOperation);
        }
    }
}