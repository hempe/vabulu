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
        public List<string> Roles { get; set; }
    }

    [Route("api/admin/user")]
    [Authorize(Roles = "admin")]
    public class AdminUserController : BaseController {
        public AdminUserController(UserManager<User> userManager, TableStore tableStore) : base(userManager, tableStore) { }

        [HttpGet("{userId}")]
        //[ProducesResponseType(typeof(string[]), 200)]
        public async Task<IActionResult> Get([FromRoute] string userId) {
            User user = await this.TableStore.GetAsync(Args<Tables.UserEntity>.Where(x => x.UserId, userId));
            var roles = await this.TableStore.GetAllAsync(Args<Tables.UserRoleEntity>.Where(x => x.UserId, userId));
            return this.Ok();
        }

        [HttpPost("")]
        //[ProducesResponseType(typeof(CalendarEvent), 200)]
        public async Task<IActionResult> Post([FromRoute] string propertyId, [FromBody] CalendarEvent data) {
            /*
            if (data == null)
                return this.BadRequest("Failed to save data.");

            if (string.IsNullOrWhiteSpace(data.Id))
                data.Id = Guid.NewGuid().ToString();

            if (data.PropertyId != propertyId)
                data.PropertyId = propertyId;

            var result = await this.TableStore.AddOrUpdateAsync<Tables.CalendarEvent>(data);
            if (result.Success())
                return this.Ok(data);
            return this.BadRequest("Failed to save data.");
            */
            return this.BadRequest();
        }

        [HttpDelete("{userId}")]
        //[ProducesResponseType(200)]
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