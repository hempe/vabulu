using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Vabulu.Tables;

namespace Vabulu.Services
{
    internal class RoleStore : IRoleStore<IdentityRole>
    {
        private readonly TableStore tableStore;
        public RoleStore(TableStore tableStore)
        {
            this.tableStore = tableStore;
        }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            var result = await this.tableStore.AddOrUpdateAsync<RoleEntity>(role);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return IdentityResult.Success;
            return IdentityResult.Failed();
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            var result = await this.tableStore.DeleteAsync<RoleEntity>(role);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return IdentityResult.Success;
            return IdentityResult.Failed();
        }

        public void Dispose() { }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            var entity = await this.tableStore.GetAsync(new RoleEntity { Id = roleId });
            return entity;
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            var entity = await this.tableStore.GetAsync(Args<RoleEntity>.Where(x => x.NormalizedName, normalizedRoleName));
            return entity;
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            var result = await this.tableStore.AddOrUpdateAsync<RoleEntity>(role);
            if (result.HttpStatusCode >= 200 && result.HttpStatusCode < 300)
                return IdentityResult.Success;
            return IdentityResult.Failed();
        }
    }
}