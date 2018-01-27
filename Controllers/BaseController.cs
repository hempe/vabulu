using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vabulu.Models;
using Vabulu.Services;

namespace Vabulu.Controllers {
    public class BaseController : Controller {
        protected UserManager<User> UserManager { get; private set; }
        protected TableStore TableStore { get; private set; }

        public BaseController(UserManager<User> userManager, TableStore tableStore) {
            this.UserManager = userManager;
            this.TableStore = tableStore;
        }

        protected string UserId { get => this.UserManager.GetUserId(this.User); }
    }
}