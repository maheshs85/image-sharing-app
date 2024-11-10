using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageSharingWithCloud.DAL;
using ImageSharingWithCloud.Models;
using ImageSharingWithCloud.Models.ViewModels;
using ImageSharingWithSecurity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace ImageSharingWithCloud.Controllers
{
    // TODO require authorization
    [Authorize]
    public class AccountController : BaseController
    {
        private readonly SignInManager<ApplicationUser> _signInManager;

        private readonly ILogger<AccountController> _logger;

        // Dependency injection of DB context and user/signin managers
        public AccountController(UserManager<ApplicationUser> userManager, 
                                 SignInManager<ApplicationUser> signInManager, 
                                 ApplicationDbContext db,
                                 IImageStorage imageStorage,
                                 ILogger<AccountController> logger) 
            : base(userManager, imageStorage, db)
        {
            this._signInManager = signInManager;
            this._logger = logger;
        }

        // TODO allow anonymous
        [HttpGet]
        [AllowAnonymous]
        public ActionResult Register()
        {
            CheckAda();
            return View();
        }

        // TODO allow anonymous, prevent CSRF
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterModel model)
        {
            CheckAda();

            if (ModelState.IsValid)
            {
                _logger.LogDebug("Registering user: {email}", model.Email);
                // Register the user from the model, and log them in

                var user = new ApplicationUser(model.Email, "on".Equals(model.Ada));
                var result = await UserManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogDebug("...registration succeeded.");
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home", new { UserName = model.Email });
                }
                else
                {
                    _logger.LogDebug("...registration failed.");
                    ModelState.AddModelError(string.Empty, "Registration failed");
                }

            }

            // If we got this far, something failed, redisplay form
            return View(model);

        }

        // TODO allow anonymous
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string returnUrl)
        {
            CheckAda();
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // TODO allow anonymous, prevent CSRF
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string returnUrl)
        {
            CheckAda();
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            _logger.LogDebug("Logging in user " + model.UserName);

            /*
             * Log in the user from the model (make sure they are still active)
             */

            ApplicationUser theUser = null;
            // TODO Use UserManager to obtain the user record from the database.
            theUser = await UserManager.FindByNameAsync(model.UserName);
            if (theUser != null && theUser.Active)
            {
                SignInResult result = null;
                // TODO Use SignInManager to log in the user.
                result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    SaveAdaCookie(theUser.Ada);
                    _logger.LogDebug("Successful login, redirecting to " + returnUrl);
                    return Redirect(returnUrl ?? "/");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Login failed");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No such user");
            }

            return View(model);
        }

        // TODO
        [HttpGet]        
        public ActionResult Password(PasswordMessageId? message)
        {
            CheckAda();
            ViewBag.StatusMessage =
                 message == PasswordMessageId.ChangePasswordSuccess ? "Your password has been changed."
                 : message == PasswordMessageId.SetPasswordSuccess ? "Your password has been set."
                 : message == PasswordMessageId.RemoveLoginSuccess ? "The external login was removed."
                 : "";
            ViewBag.ReturnUrl = Url.Action("Password");
            return View();
        }

        // TODO prevent CSRF
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Password(LocalPasswordModel model)
        {
            CheckAda();

            ViewBag.ReturnUrl = Url.Action("Password");
            if (ModelState.IsValid)
            {
                /*
                 * Change the password
                 */
                ApplicationUser user = await GetLoggedInUser();
                //string resetToken = await userManager.GeneratePasswordResetTokenAsync(user);
                //idResult = await userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);
                await UserManager.RemovePasswordAsync(user);
                IdentityResult idResult = await UserManager.AddPasswordAsync(user, model.NewPassword);

                if (idResult.Succeeded)
                {
                    return RedirectToAction("Password", new { Message = PasswordMessageId.ChangePasswordSuccess });
                }
                else
                {
                    ModelState.AddModelError("", "The new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        // TODO require Admin permission
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Manage()
        {
            CheckAda();

            List<SelectListItem> users = [];
            foreach (var u in Db.Users)
            {
                SelectListItem item = new() { Text = u.UserName, Value = u.Id, Selected = u.Active };
                users.Add(item);
            }

            ViewBag.message = "";
            ManageModel model = new() { Users = users };
            return View(model);
        }

        // TODO require Admin permission, prevent CSRF
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Manage(ManageModel model)
        {
            CheckAda();

            foreach (var userItem in model.Users)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(userItem.Value);

                // Need to reset username in view model before returning to user, it is not posted back
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                userItem.Text = user.UserName;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                if (user.Active && !userItem.Selected)
                {
                    await ImageStorage.RemoveImagesAsync(user);
                    user.Active = false;
                }
                else if (!user.Active && userItem.Selected)
                {
                    /*
                     * Reactivate a user
                     */
                    user.Active = true;
                }
            }
            await Db.SaveChangesAsync();

            ViewBag.message = "Users successfully deactivated/reactivated";

            return View(model);
        }

        // TODO
        public async Task<IActionResult> Logout()
        {
            CheckAda();

            ApplicationUser user = await GetLoggedInUser();
            return View(new LogoutModel { UserName = user.UserName });
        }

        // TODO
        public async Task<IActionResult> DoLogout()
        {
            CheckAda();

            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // TODO
        public IActionResult AccessDenied(string returnUrl)
        {
            CheckAda();

            return View(new AccessDeniedModel
            {
                ReturnUrl = returnUrl
            });
        }

        private void SaveAdaCookie(bool value)
        {
            // Save the value in a cookie field key
            var options = new CookieOptions()
            {
                IsEssential = true,
                Expires = DateTime.Now.AddMonths(3)
            };
            Response.Cookies.Append("ADA", value ? "true" : "false", options);
        }

        public enum PasswordMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

    }
}
