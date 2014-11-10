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
            return Insert(userNamesIndexTable, entity);
        }

        public Task<TableResult> InsertUserEmailsIndexTableEntity(UserEmailIndex entity)
        {
            return Insert(userEmailsIndexTable, entity);
        }

        public Task<TableResult> InsertOrReplaceUserTableEntity(User entity)
        {
            return InsertOrReplace(usersTable, entity);
        }

        public Task<TableResult> DeleteUserTableEntity(User entity)
        {
            return Delete(usersTable, entity);
        }

        public Task<TableResult> DeleteUserNamesIndexTableEntity(UserNameIndex entity)
        {
            return Delete(userNamesIndexTable, entity);
        }

        public Task<TableResult> DeleteUserEmailsIndexTableEntity(UserEmailIndex entity)
        {
            return Delete(userEmailsIndexTable, entity);
        }

        public Task<TableResult> UpdateUserTableEntity(User entity)
        {
            return Replace(usersTable, entity);
        }

        public Task<IEnumerable<UserEmailIndex>> ExecuteQueryOnUserEmailsIndex(TableQuery<UserEmailIndex> query)
        {
            return Task.Factory.StartNew(() => userEmailsIndexTable.ExecuteQuery(query));
        }

        public Task<IEnumerable<T>> ExecuteQueryOnUser<T>(TableQuery<T> query) where T : User, new()
        {
            return Task.Factory.StartNew(() => usersTable.ExecuteQuery(query));
        }

        public Task<IEnumerable<UserNameIndex>> ExecuteQueryOnUserNamesIndex(TableQuery<UserNameIndex> query)
        {
            return Task.Factory.StartNew(() => userNamesIndexTable.ExecuteQuery(query));
        }

        public Task<IEnumerable<UserExternalLoginIndex>> ExecuteQueryOnUserExternalLoginsIndex(TableQuery<UserExternalLoginIndex> query)
        {
            return Task.Factory.StartNew(() => userExternalLoginsIndexTable.ExecuteQuery(query));
        }

        private Task<TableResult> Insert(CloudTable table, ITableEntity entity)
        {
            var insertOperation = TableOperation.Insert(entity);
            return table.ExecuteAsync(insertOperation);
        }

        private Task<TableResult> InsertOrReplace(CloudTable table, ITableEntity entity)
        {
            var insertOrReplaceOperation = TableOperation.InsertOrReplace(entity);
            return table.ExecuteAsync(insertOrReplaceOperation);
        }

        private Task<TableResult> Delete(CloudTable table, ITableEntity entity)
        {
            entity.ETag = "*";
            var deleteOperation = TableOperation.Delete(entity);
            return table.ExecuteAsync(deleteOperation);
        }

        private Task<TableResult> Replace(CloudTable table, ITableEntity entity)
        {
            entity.ETag = "*";
            var replaceOperation = TableOperation.Replace(entity);
            return table.ExecuteAsync(replaceOperation);
        }
    }
}