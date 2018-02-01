using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using Vabulu.Models;

namespace Vabulu.Services {

    public class WarmUpOptions {
        public List<string> AdminUsers { get; set; }
    }

    public class WarmUp {

        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly TableStore tableStore;
        private WarmUpOptions options;
        public WarmUp(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IOptions<WarmUpOptions> options, TableStore tableStore) {
            this.userManager = userManager;
            this.options = options.Value;
            this.tableStore = tableStore;
            this.roleManager = roleManager;
        }
        public void Load() {
            this.LoadAsync().GetAwaiter().GetResult();
        }

        private async Task LoadAsync() {
            if (this.options == null || this.options.AdminUsers == null)
                return;

            await this.roleManager.CreateAsync(new IdentityRole("admin"));
            await this.roleManager.CreateAsync(new IdentityRole("user"));
            await this.roleManager.CreateAsync(new IdentityRole("edit"));

            foreach (var admin in this.options.AdminUsers) {
                var user = await this.userManager.FindByNameAsync(admin);
                if (user == null)
                    continue;
                await this.userManager.AddToRoleAsync(user, "admin");
            }

            var x = await this.userManager.FindByNameAsync("hempe@live.com");
            await this.userManager.AddToRoleAsync(x, "user");

        }

        private async Task RemoveOldRoleAssignementAsync() {
            var roles = await this.tableStore.GetAllAsync<Tables.RoleEntity>();
            foreach (var r in roles)
                await this.tableStore.DeleteAsync(r);

            var userroles = await this.tableStore.GetAllAsync<Tables.UserRoleEntity>();
            foreach (var r in userroles)
                await this.tableStore.DeleteAsync(r);
        }
    }
}