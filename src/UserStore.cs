using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AdaptiveSystems.AspNetIdentity.AzureTableStorage.Exceptions;
using Microsoft.AspNet.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NExtensions;

namespace AdaptiveSystems.AspNetIdentity.AzureTableStorage
{
    public class UserStore<T> : IUserStore<T>, IUserPasswordStore<T>, IUserEmailStore<T>, IUserLockoutStore<T, string>, IDisposable where T : User, new()
    {
        private readonly CloudTable usersTable;
        private readonly CloudTable userNamesIndexTable;
        private readonly CloudTable emailIndexTable;

        public UserStore(string connectionString) : this(CloudStorageAccount.Parse(connectionString)) { }
        public UserStore(CloudStorageAccount storageAccount) : this(storageAccount, true) { }
        public UserStore(CloudStorageAccount storageAccount, bool createIfNotExists) : this(storageAccount, createIfNotExists, "users", "userNamesIndex", "emailIndex") { }
        public UserStore(CloudStorageAccount storageAccount, bool createIfNotExists, string usersTableName, string userNamesIndexTableName, string emailIndexTableName)
        {
            CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

            usersTable = tableClient.GetTableReference(usersTableName);
            userNamesIndexTable = tableClient.GetTableReference(userNamesIndexTableName);
            emailIndexTable = tableClient.GetTableReference(emailIndexTableName);

            if (createIfNotExists)
            {
                usersTable.CreateIfNotExists();
                userNamesIndexTable.CreateIfNotExists();
                emailIndexTable.CreateIfNotExists();
            }
        }

        public static UserStore<T> Create()
        {
            return new UserStore<T>(ConfigurationManager.ConnectionStrings["UserStore.ConnectionString"].ConnectionString);
        }

        private async Task CreateUserNameIndex(T user)
        {
            var userNameIndex = new UserNameIndex(user.UserName.Base64Encode(), user.Id);
            var insertUserNameIndexOperation = TableOperation.Insert(userNameIndex);

            try
            {
                await userNamesIndexTable.ExecuteAsync(insertUserNameIndexOperation);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 409)
                {
                    throw new DuplicateUsernameException();
                }
                throw;
            }
        }

        private async Task CreateEmailIndex(T user)
        {
            var emailIndex = new EmailIndex(user.Email.Base64Encode(), user.Id);
            var insertEmailIndexIOperation = TableOperation.Insert(emailIndex);

            try
            {
                await emailIndexTable.ExecuteAsync(insertEmailIndexIOperation);
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 409)
                {
                    throw new DuplicateEmailException();
                }
                throw;
            }
        }

        public async Task CreateAsync(T user)
        {
            user.ThrowIfNull("user");
            user.SetPartionAndRowKeys();

            await CreateUserNameIndex(user);
            await CreateEmailIndex(user);

            try
            {
                var insertOrReplaceUserOperation = TableOperation.InsertOrReplace(user);
                await usersTable.ExecuteAsync(insertOrReplaceUserOperation);
            }
            catch (Exception)
            {
                // attempt to delete the index item - needs work
                RemoveIndices(user).Wait();//cannt await in the catch of a try block so have to wait
                throw;
            }
        }

        public async Task DeleteAsync(T user)
        {
            user.ThrowIfNull("user");

            var operation = TableOperation.Delete(user);
            user.ETag = "*";
            await usersTable.ExecuteAsync(operation);

            await RemoveIndices(user);
        }

        public Task<T> FindByIdAsync(string userId)
        {
            userId.ThrowIfNullOrEmpty("userId");

            return Task.Factory.StartNew(() =>
            {
                var query = new TableQuery<T>()
                            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, userId))
                            .Take(1);
                var results = usersTable.ExecuteQuery(query);
                return results.SingleOrDefault();
            });
        }

        public Task<T> FindByNameAsync(string userName)
        {
            userName.ThrowIfNullOrEmpty("userName");

            return Task.Factory.StartNew(() =>
            {
                var indexQuery = new TableQuery<UserNameIndex>()
                                .Where(TableQuery.GenerateFilterCondition("PartitionKey", 
                                        QueryComparisons.Equal, userName.Base64Encode()))
                                .Take(1);
                var indexResults = userNamesIndexTable.ExecuteQuery(indexQuery);
                var indexItem = indexResults.SingleOrDefault();

                if (indexItem == null)
                {
                    return null;
                }

                return FindByIdAsync(indexItem.UserId).Result;
            });
        }

        public async Task UpdateAsync(T user)
        {
            user.ThrowIfNull("user");

            // assumption here is that a username can't change (if it did we'd need to fix the index)
            user.ETag = "*";
            TableOperation operation = TableOperation.Replace(user);
            await usersTable.ExecuteAsync(operation);
        }

        public void Dispose()
        {
            
        }

        public Task<string> GetPasswordHashAsync(T user)
        {
            user.ThrowIfNull("user");

            return Task.FromResult(user.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(T user)
        {
            user.ThrowIfNull("user");

            return Task.FromResult(user.PasswordHash.HasValue());
        }

        public Task SetPasswordHashAsync(T user, string passwordHash)
        {
            user.ThrowIfNull("user");
            passwordHash.ThrowIfNullOrEmpty("passwordHash");

            user.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<T> FindByEmailAsync(string email)
        {
            email.ThrowIfNullOrEmpty("email");

            return Task.Factory.StartNew(() =>
            {
                var indexQuery = new TableQuery<EmailIndex>()
                                .Where(TableQuery.GenerateFilterCondition("PartitionKey",
                                        QueryComparisons.Equal, email.Base64Encode()))
                                .Take(1);
                var indexResults = emailIndexTable.ExecuteQuery(indexQuery);
                var indexItem = indexResults.SingleOrDefault();

                if (indexItem == null)
                {
                    return null;
                }

                return FindByIdAsync(indexItem.UserId).Result;
            });

        }

        public Task<string> GetEmailAsync(T user)
        {
            user.ThrowIfNull("user");

            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(T user)
        {
            user.ThrowIfNull("user");

            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailAsync(T user, string email)
        {
            user.ThrowIfNull("user");
            email.ThrowIfNullOrEmpty("email");

            user.Email = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(T user, bool confirmed)
        {
            user.ThrowIfNull("user");

            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        private async Task RemoveIndices(T user)
        {
            var userNameIndex = new UserNameIndex(user.UserName.Base64Encode(), user.Id);
            userNameIndex.ETag = "*";

            var emailIndex = new EmailIndex(user.Email.Base64Encode(), user.Id);
            emailIndex.ETag = "*";
            
            var t1 = userNamesIndexTable.ExecuteAsync(TableOperation.Delete(userNameIndex));
            var t2 = emailIndexTable.ExecuteAsync(TableOperation.Delete(emailIndex));

            await Task.WhenAll(t1, t2);
        }


        public Task<int> GetAccessFailedCountAsync(T user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> GetLockoutEnabledAsync(T user)
        {
            user.ThrowIfNull("user");
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(T user)
        {
            user.ThrowIfNull("user");
            return Task.FromResult((DateTimeOffset)DateTime.SpecifyKind(user.LockoutEndDate ?? new DateTime(1601, 1, 1), DateTimeKind.Utc));
        }

        public Task<int> IncrementAccessFailedCountAsync(T user)
        {
            user.ThrowIfNull("user");
            user.AccessFailedCount++;
            return Task.FromResult(0);
        }

        public Task ResetAccessFailedCountAsync(T user)
        {
            user.ThrowIfNull("user");
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(T user, bool enabled)
        {
            user.ThrowIfNull("user");
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task SetLockoutEndDateAsync(T user, DateTimeOffset lockoutEnd)
        {
            user.ThrowIfNull("user");

            user.LockoutEndDate = lockoutEnd.UtcDateTime;
            return Task.FromResult(0);
        }
    }
}
