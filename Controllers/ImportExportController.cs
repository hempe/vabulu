using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vabulu.Models;
using Vabulu.Services;
using Vabulu.Services.Export;

namespace Vabulu.Controllers {

    [Route("api/export")]
    [Authorize(Roles = "user, edit, admin")]
    public class ImportController : BaseController {
        public ImportController(UserManager<User> userManager, TableStore tableStore) : base(userManager, tableStore) { }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Export(
            [FromServices] BaseHandler json, [FromServices] XlsHandler xls, [FromServices] HtmlHandler html, [FromQuery] string format
        ) {
            try {
                IActionResult file = null;
                switch (format.ToLower()) {
                    case "xlsx":
                    case "xls":
                        file = this.File(await xls.GetExportAsync(this.UserId), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
                        break;
                    case "html":
                        file = this.File(await html.GetExportAsync(this.UserId), "text/html");
                        break;
                    default:
                        file = this.File(await json.GetExportAsync(this.UserId), "application/json");
                        break;
                }
                return file;
            } catch (Exception e) {
                return this.Ok(e);
            }
        }
    }
}