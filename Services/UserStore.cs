using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Vabulu.Attributes;
using Vabulu.Middleware;
using Vabulu.Models;
using Vabulu.Tables;

namespace Vabulu.Services {

    internal partial class UserStore : IUserStore<User> {
        private static List<Type> Types;
        private readonly TableStore tableStore;

        static UserStore() {
            Types = typeof(UserStore).Assembly.GetTypes().Where(t => typeof(UserData).IsAssignableFrom(t)).Where(t => t.IsTable()).ToList();
        }

        public UserStore(TableStore tableStore) {
            this.tableStore = tableStore;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken) {
            var result = await this.tableStore.AddOrUpdateAsync<UserEntity>(user);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return IdentityResult.Success;
            return IdentityResult.Failed();
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken) {

            var logins = await this.GetLoginsAsync(user, cancellationToken);
            foreach (var l in logins) {
                await this.RemoveLoginAsync(user, l.LoginProvider, l.ProviderKey, cancellationToken);
            }
            var tokens = await this.GetTokensAsync(user);
            foreach (var t in tokens) {
                await this.RemoveTokenAsync(user, t.PartitionKey, t.Name, cancellationToken);
            }

            foreach (var type in Types) {
                try {

                    var entities = await this.tableStore.GetAllAsync(type, new Args { { nameof(UserData.UserId), user.Id } });
                    foreach (var e in entities) {
                        await this.tableStore.DeleteAsync(type, e);
                    }
                } catch { }
            }

            try {
                await this.tableStore.DeleteAsync<UserEntity>(user);
            } catch { }
            return IdentityResult.Success;

        }

        public void Dispose() { }

        public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken) {
            var entity = await this.tableStore.GetAsync(new UserEntity { UserId = userId });
            return entity;
        }

        public async Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken) {
            var entity = await this.tableStore.GetAsync<UserEntity>(
                new Args { { nameof(UserEntity.NormalizedUserName), normalizedUserName }
                });
            return entity;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken) {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken) {
            return Task.FromResult<string>(user.Id);
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken) {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken) {
            user.NormalizedUserName = user.UserName.ToUpper();
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken) {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken) {
            var result = await this.tableStore.AddOrUpdateAsync<UserEntity>(user);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return IdentityResult.Success;
            return IdentityResult.Failed();
        }

        private async Task<List<Token>> GetTokensAsync(User user) {
            var entities = await this.tableStore.GetAllAsync<Token>(
                new Args { { nameof(Token.UserId), user.Id } }
            );
            return entities;
        }
    }
}