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

    [Route("api/admin/property")]
    [Authorize(Roles = "admin")]
    public class AdminEventController : BaseController {
        public AdminEventController(UserManager<User> userManager, TableStore tableStore) : base(userManager, tableStore) { }

        [HttpGet("{propertyId}/events")]
        [ProducesResponseType(typeof(CalendarEvent[]), 200)]
        public async Task<IActionResult> Get([FromRoute] string propertyId) {
            var values = await this.TableStore.GetAllAsync<Tables.CalendarEvent>(new Args { { nameof(Tables.CalendarEvent.PropertyId), propertyId } });
            return this.Ok(values.Select(x =>(CalendarEvent) x));
        }

        [HttpPost("{propertyId}/events")]
        [ProducesResponseType(typeof(CalendarEvent), 200)]
        public async Task<IActionResult> Post([FromRoute] string propertyId, [FromBody] CalendarEvent data) {
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
        }

        [HttpDelete("{propertyId}/events/{id}")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> Delete([FromRoute] string propertyId, [FromRoute] string id) {
            await this.TableStore.DeleteAsync(new Tables.CalendarEvent { Id = id, PropertyId = propertyId });
            return this.Ok();
        }
    }
}