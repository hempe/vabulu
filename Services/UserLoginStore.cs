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

    internal partial class UserStore : IUserLoginStore<User> {

        public async Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken) {

            LoginInfo entity = login;
            entity.UserId = user.Id;

            var result = await this.tableStore.AddOrUpdateAsync(entity);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return;
            throw new Exception("Update failed");
        }

        public async Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken) {
            var entity = await this.tableStore.GetAsync<LoginInfo>(new LoginInfo { LoginProvider = loginProvider, ProviderKey = providerKey });

            if (entity == null)
                return null;

            return await this.FindByIdAsync(entity.UserId, cancellationToken);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken) {
            var entities = await this.tableStore.GetAllAsync<LoginInfo>(
                new Args { { nameof(LoginInfo.UserId), user.Id } }
            );

            return entities.Select(x =>(UserLoginInfo) x).ToList();
        }

        public async Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken) {
            try {
                await this.tableStore.DeleteAsync<UserEntity>(user);
            } catch { }
        }
    }
}