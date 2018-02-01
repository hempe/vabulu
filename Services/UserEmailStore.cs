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

    internal partial class UserStore : IUserEmailStore<User> {
        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken) {
            var entity =
                await this.tableStore.GetAsync(Args<UserEntity>.Where(x => x.NormalizedEmail, normalizedEmail)) ??
                await this.tableStore.GetAsync(Args<UserEntity>.Where(x => x.Email, normalizedEmail)) ??
                await this.tableStore.GetAsync(Args<UserEntity>.Where(x => x.NormalizedUserName, normalizedEmail));
            return entity;
        }

        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.Email);

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.EmailConfirmed);

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken) => Task.FromResult(user.NormalizedEmail);

        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken) {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken) {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken) {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

    }
}