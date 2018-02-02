using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json.Linq;
using Vabulu.Attributes;
using Vabulu.Middleware;
using Vabulu.Models;
using Vabulu.Services;
using Vabulu.Services.I18n;
using Vabulu.Views;

namespace Vabulu.Controllers {

    public class EditUser {
        public string Id { get; set; }
        public string UserName { get; set; }
        public List<string> Roles { get; set; }
    }

    public class QueryUser {
        public string Id { get; set; }
        public string UserName { get; set; }
    }

    [Route("api/admin/user")]
    [Authorize(Roles = "admin")]
    public class AdminUserController : BaseController {
        public AdminUserController(UserManager<User> userManager, TableStore tableStore) : base(userManager, tableStore) { }

        [HttpGet("")]
        [ProducesResponseType(typeof(QueryUser[]), 200)]
        public async Task<IActionResult> GetAll() {
            var users = await this.TableStore.GetAllAsync<Tables.UserEntity>();
            return this.Ok(users.Select(x => new QueryUser {
                Id = x.UserId,
                    UserName = x.UserName
            }));
        }

        [HttpGet("roles")]
        [ProducesResponseType(typeof(string[]), 200)]
        public async Task<IActionResult> GetRoles() {
            var roles = await this.TableStore.GetAllAsync<Tables.RoleEntity>();
            return this.Ok(roles.Select(x => x.Name));
        }

        [HttpGet("{userId}")]
        [ProducesResponseType(typeof(EditUser), 200)]
        public async Task<IActionResult> Get([FromRoute] string userId) {
            User user = await this.TableStore.GetAsync(Args<Tables.UserEntity>.Where(x => x.UserId, userId));
            var roles = await this.TableStore.GetAllAsync(Args<Tables.UserRoleEntity>.Where(x => x.UserId, userId));
            return this.Ok(new EditUser {
                Id = user.Id,
                    UserName = user.UserName,
                    Roles = roles.Select(x => x.RoleName).ToList()
            });
        }

        [HttpPost("")]
        [ProducesResponseType(typeof(EditUser), 200)]
        public async Task<IActionResult> Post([FromBody] EditUser data) {

            if (data == null)
                return this.BadRequest("Failed to save data.");

            var user = await this.UserManager.FindByIdAsync(data.Id);
            if (User == null)
                return this.BadRequest();

            if (user.Id == this.UserId)
                return this.BadRequest();

            var roles = await this.TableStore.GetAllAsync(Args<Tables.UserRoleEntity>.Where(x => x.UserId, data.Id));
            var remove = roles.Select(x => x.RoleName).Except(data.Roles).ToList();
            var add = data.Roles.Except(roles.Select(x => x.RoleName)).ToList();

            await this.UserManager.RemoveFromRolesAsync(user, remove);
            await this.UserManager.AddToRolesAsync(user, add);

            return await this.Get(user.Id);
        }

        [HttpDelete("{userId}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete([FromRoute] string userId) {

            var user = await this.UserManager.FindByIdAsync(userId);
            if (user == null)
                return this.BadRequest();
            if (user.Id == this.UserId)
                return this.BadRequest();

            var result = await this.UserManager.DeleteAsync(user);
            if (result.Succeeded)
                return this.Ok();

            return this.BadRequest(result.Errors);
        }
    }
}