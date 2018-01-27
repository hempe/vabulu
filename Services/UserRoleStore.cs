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

    internal partial class UserStore : IUserRoleStore<User> {

        public async Task AddToRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken) {
            var roleName = await this.GetRoleNameAsync(normalizedRoleName);
            var result = await this.tableStore.AddOrUpdateAsync(new UserRoleEntity { UserId = user.Id, RoleName = roleName });
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return;
            throw new Exception("Update failed");
        }

        public async Task<IList<string>> GetRolesAsync(User user, CancellationToken cancellationToken) {
            var entities = await this.tableStore.GetAllAsync<UserRoleEntity>(new Args { { nameof(UserRoleEntity.UserId), user.Id } });
            if (entities == null)
                return null;
            var roles = new List<string>();
            return entities.Select(x => x.RoleName).ToList();
        }

        public async Task<IList<User>> GetUsersInRoleAsync(string normalizedRoleName, CancellationToken cancellationToken) {
            var roleName = await this.GetRoleNameAsync(normalizedRoleName);
            var entities = await this.tableStore.GetAllAsync<UserRoleEntity>(new Args { { nameof(UserRoleEntity.RoleName), roleName } });
            if (entities == null)
                return null;
            var users = new List<User>();
            foreach (var entity in entities) {
                users.Add(await this.FindByIdAsync(entity.UserId, cancellationToken));
            }

            return users;
        }

        public async Task<bool> IsInRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken) {
            var roleName = await this.GetRoleNameAsync(normalizedRoleName);
            var entity = await this.tableStore.GetAsync(new UserRoleEntity { UserId = user.Id, RoleName = roleName });
            return entity != null;
        }

        public async Task RemoveFromRoleAsync(User user, string normalizedRoleName, CancellationToken cancellationToken) {
            var roleName = await this.GetRoleNameAsync(normalizedRoleName);
            var result = await this.tableStore.DeleteAsync(new UserRoleEntity { UserId = user.Id, RoleName = roleName });
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return;
            throw new Exception("Update failed");
        }

        private async Task<string> GetRoleNameAsync(string normalizedRoleName) {
            var entity = await this.tableStore.GetAsync<RoleEntity>(
                new Args { { nameof(RoleEntity.NormalizedName), normalizedRoleName }
                });
            return entity?.Name ?? normalizedRoleName;
        }
    }
}