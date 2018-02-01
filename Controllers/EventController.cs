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

    [Route("api/property")]
    [Authorize(Roles = "user, edit, admin")]
    public class EventController : BaseController {
        public EventController(UserManager<User> userManager, TableStore tableStore) : base(userManager, tableStore) { }

        [HttpGet("{propertyId}/events")]
        [ProducesResponseType(typeof(CalendarEvent[]), 200)]
        public async Task<IActionResult> Get([FromRoute] string propertyId) {
            var values = await this.TableStore.GetAllAsync(Args<Tables.CalendarEvent>.Where(x => x.PropertyId, propertyId));
            return this.Ok(values.Select(x => new CalendarEvent {
                Start = x.Start,
                    End = x.End,
                    PropertyId = x.PropertyId,
                    Color = x.Color
            }));
        }
    }
}