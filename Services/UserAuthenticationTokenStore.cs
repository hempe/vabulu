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

    internal partial class UserStore : IUserAuthenticationTokenStore<User> {
        public async Task RemoveTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken) {
            var result = await this.tableStore.DeleteAsync(new Token {
                LoginProvider = loginProvider,
                    UserId = user.Id,
                    Name = name
            });
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                throw new Exception("Update failed");
        }

        public async Task SetTokenAsync(User user, string loginProvider, string name, string value, CancellationToken cancellationToken) {
            var entity = new Token {
                LoginProvider = loginProvider,
                UserId = user.Id,
                Name = name,
                Value = value
            };

            var result = await this.tableStore.AddOrUpdateAsync(entity);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return;

            throw new Exception("Update failed");
        }

        public async Task<string> GetTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken) {
            var entity = await this.tableStore.GetAsync(new Token {
                LoginProvider = loginProvider,
                    UserId = user.Id,
                    Name = name
            });

            return entity?.Value;
        }
    }
}