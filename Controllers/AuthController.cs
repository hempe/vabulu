using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Flurl;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Vabulu.Models;
using Vabulu.Models.Auth;
using Vabulu.Services;
using Vabulu.Services.I18n;

namespace Vabulu.Controllers {

    [Route(".auth")]
    public class AuthController : BaseController {
        private readonly SignInManager<User> signInManager;
        public AuthController(UserManager<User> userManager, TableStore tableStore, SignInManager<User> signInManager) : base(userManager, tableStore) {
            this.signInManager = signInManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index() {
            var schemes = await this.signInManager.GetExternalAuthenticationSchemesAsync();
            var loginProviders = schemes.Select(x => new { x.Name, x.DisplayName }).ToList();
            return this.Ok(loginProviders);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromServices] MailService mailService, [FromServices] TranslationService translationService, [FromBody] Register model) {
            if (this.User.Identity.IsAuthenticated)
                return this.BadRequest("Already signed in");

            if (string.IsNullOrWhiteSpace(model.Email)) {
                return this.BadRequest(new { Email = new [] { "Required" } });
            }
            if (string.IsNullOrWhiteSpace(model.Password)) {
                return this.BadRequest(new { Password = new [] { "Required" } });
            }

            var user = new User() { Email = model.Email, UserName = model.Email, Language = model.Language };
            var created = await this.UserManager.CreateAsync(user, model.Password);
            if (created.Succeeded) {
                await this.signInManager.SignInAsync(user, isPersistent : false);
                await this.RequestEmailConfirmationAsync(mailService, translationService, user);
                await this.NotifyAdminsOfRegistration(mailService, translationService, user);
                return this.Ok();
            }

            return this.BadRequest(new {
                Email = created.Errors.Where(e => e.Code.ToLower().Contains("email")).Select(x => x.Code).ToList(),
                    Password = created.Errors.Where(e => e.Code.ToLower().Contains("password")).Select(x => x.Code).ToList(),
            });
        }

        [AllowAnonymous]
        [HttpPost("signin")]
        public async Task<IActionResult> Login([FromBody] Login model) {
            if (this.User.Identity.IsAuthenticated)
                return this.BadRequest("Already signed in");

            // lockoutOnFailure: true
            var result = await this.signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure : false);
            if (result.Succeeded) {
                return this.Ok();
            }
            if (result.RequiresTwoFactor) {
                return this.BadRequest(new {
                    Email = new [] { "TwoFactorNotSupported" }
                });
            }
            if (result.IsLockedOut) {
                return this.BadRequest(new {
                    Email = new [] { "Lockout" }
                });
            } else {
                return this.BadRequest(new {
                    Email = new [] { "InvalidLoginAttempt" }
                });
            }
        }

        [AllowAnonymous, Route("error/{code}"), HttpGet, HttpDelete, HttpPost, HttpPut, HttpPatch]
        public IActionResult accessdenied([FromRoute] int code) {
            return this.StatusCode(code);
        }

        [AllowAnonymous]
        [HttpGet("self")]
        public IActionResult UserInfo() {
            if (!this.User.Identity.IsAuthenticated)
                return this.Ok(new {
                    loggedIn = false,
                });

            return this.Ok(new {
                loggedIn = true,
                    UserName = this.User.Identity.Name,
                    Roles = this.User.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList()
            });
        }

        [HttpGet("signout")]
        public async Task<IActionResult> SignOut([FromQuery] string returnUrl = null) {
            await this.signInManager.SignOutAsync();
            return this.RedirectToLocal(returnUrl);
        }

        [Authorize]
        [HttpGet("iframe")]
        public IActionResult IFrame() => this.Ok();

        [AllowAnonymous]
        [HttpGet("signin/{provider}")]
        public async Task<IActionResult> ExternalLogin([FromRoute] string provider, [FromQuery] string returnUrl = null) {
            var schemes = await this.signInManager.GetExternalAuthenticationSchemesAsync();
            var schema = schemes.Where(x => string.Equals(provider, x.Name, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
            var url = "/.auth/signin/callback".SetQueryParam("returnUrl", returnUrl);
            var properties = this.signInManager.ConfigureExternalAuthenticationProperties(schema.Name, url);
            return Challenge(properties, schema.Name);
        }

        [AllowAnonymous]
        [HttpGet("signin/callback")]
        public async Task<IActionResult> ExternalLoginCallback([FromQuery] string returnUrl = null, [FromQuery] string remoteError = null) {
            if (remoteError != null) {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return this.BadRequest("Error from external provider");
            }
            var info = await this.signInManager.GetExternalLoginInfoAsync();

            if (info == null) {
                return this.BadRequest();
            }
            var claims = info.Principal.Claims.ToList();

            var result = await this.signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent : true);
            if (result.Succeeded) {
                await this.signInManager.UpdateExternalAuthenticationTokensAsync(info);
                return this.RedirectToLocal(returnUrl);
            }
            if (result.RequiresTwoFactor) {
                return this.BadRequest("Two factor not supported");
            }
            if (result.IsLockedOut) {
                return this.BadRequest("Lockout");
            } else {

                var email = info.Principal.FindFirstValue(ClaimTypes.Email);
                var user = new User { UserName = email, Email = email, EmailConfirmed = true, };

                var created = await this.UserManager.CreateAsync(user);
                if (created.Succeeded) {
                    created = await this.UserManager.AddLoginAsync(user, info);
                    if (created.Succeeded) {
                        await this.signInManager.SignInAsync(user, isPersistent : false);
                        //_logger.LogInformation(6, "User created an account using {Name} provider.", info.LoginProvider);
                        await this.signInManager.UpdateExternalAuthenticationTokensAsync(info);
                        return this.RedirectToLocal(returnUrl);
                    }
                }

                return this.BadRequest(created.Errors);
            }
        }

        [HttpGet("confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string code) {
            if (userId == null || code == null) {
                return this.RedirectToLocal("/");
            }

            var user = await UserManager.FindByIdAsync(userId);
            if (user == null) {
                return this.RedirectToLocal("/");
            }
            var result = await UserManager.ConfirmEmailAsync(user, code);
            return this.RedirectToLocal("/");
        }

        [HttpPost("forgot")]
        [AllowAnonymous]
        public async Task<ActionResult> ForgotPassword([FromServices] MailService mailService, [FromServices] TranslationService translationService, [FromBody] ForgotPassword model) {
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null) {
                return this.Ok();
            }

            if (!await UserManager.IsEmailConfirmedAsync(user)) {
                await this.RequestEmailConfirmationAsync(mailService, translationService, user);
                return this.Ok();
            }

            string code = await UserManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = this.Request.GetDisplayUrl().ResetToRoot().SetQueryParams(new {
                forgotCode = code,
                    email = user.Email
            }).ToString();

            await mailService.SendEmailAsync(
                model.Email,
                await translationService.TranslateAsync(model.Language, "ResetPasswordTemplate.Title"),
                "Templates/ResetPassword.html.template",
                new {
                    Name = $"{user.UserName}",
                        CallbackUrl = callbackUrl,
                        Url = this.Request.GetDisplayUrl().ResetToRoot()
                }, model.Language);

            return this.Ok();
        }

        [AllowAnonymous]
        [HttpPost("reset")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPassword model) {
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null) {
                return this.Ok();
            }
            var result = await UserManager.ResetPasswordAsync(user, model.Code, model.Password);
            if (result.Succeeded) {
                return this.Ok();
            }

            return this.BadRequest(new {
                Email = result.Errors.Where(e => e.Code.ToLower().Contains("email")).Select(x => x.Code).ToList(),
                    Password = result.Errors.Where(e => e.Code.ToLower().Contains("password")).Select(x => x.Code).ToList(),
            });
        }

        private async Task RequestEmailConfirmationAsync(MailService mailService, TranslationService translationService, Models.User user) {
            try {
                string code = await UserManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = this.Request.GetDisplayUrl().ResetToRoot().AppendPathSegments(".auth", "confirm").SetQueryParams(new {
                    code = code,
                        userId = user.Id
                }).ToString();

                await mailService.SendEmailAsync(
                    user.Email,
                    await translationService.TranslateAsync(user.Language, "RegisterTemplate.Title"),
                    "Templates/Register.html.template",
                    new {
                        Name = $"{user.UserName}",
                            CallbackUrl = callbackUrl,
                            Url = this.Request.GetDisplayUrl().ResetToRoot()
                    }, user.Language);

            } catch { }
        }

        private async Task NotifyAdminsOfRegistration(MailService mailService, TranslationService translationService, Models.User user) {
            var callbackUrl = this.Request.GetDisplayUrl().ResetToRoot().AppendPathSegments("user", user.Id);
            var url = this.Request.GetDisplayUrl().ResetToRoot();
            var userroles = await this.TableStore.GetAllAsync(Args<Tables.UserRoleEntity>.Where(x => x.RoleName, "admin"));
            foreach (var ur in userroles) {
                var admin = await this.UserManager.FindByIdAsync(ur.UserId);
                await mailService.SendEmailAsync(
                    user.Email,
                    await translationService.TranslateAsync(admin.Language, "RegistrationTemplate.Title"),
                    "Templates/Registration.html.template",
                    new {
                        Name = $"{user.UserName}",
                            CallbackUrl = callbackUrl,
                            Email = user.Email,
                            Url = url
                    }, admin.Language);
            }
        }

        private IActionResult RedirectToLocal(string returnUrl) {
            if (Url.IsLocalUrl(returnUrl)) {
                return Redirect(returnUrl);
            } else {
                return Redirect("~/");
            }
        }
    }
}