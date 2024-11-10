using Microsoft.AspNetCore.Mvc;
using ImageSharingWithCloud.DAL;
using ImageSharingWithCloud.Models;
using ImageSharingWithCloud.Models.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Azure;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ImageSharingWithCloud.Controllers
{
    // TODO require authorization by default
    public class ImagesController : BaseController
    {
        private readonly ILogContext _logContext;

        private readonly ILogger<ImagesController> _logger;

        // Dependency injection
        public ImagesController(UserManager<ApplicationUser> userManager,
                                ApplicationDbContext userContext,
                                ILogContext logContext,
                                IImageStorage imageStorage,
                                ILogger<ImagesController> logger)
            : base(userManager, imageStorage, userContext)
        {
            this._logContext = logContext;

            this._logger = logger;
        }


        // TODO

        public ActionResult Upload()
        {
            CheckAda();

            ViewBag.Message = "";
            ImageView imageView = new ImageView();
            return View(imageView);
        }

        // TODO prevent CSRF
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> Upload(ImageView imageView)
        {
            CheckAda();

            _logger.LogDebug("Processing the upload of an image....");

            await TryUpdateModelAsync(imageView);

            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Please correct the errors in the form!";
                return View();
            }

            _logger.LogDebug("...getting the current logged-in user....");
            ApplicationUser user = await GetLoggedInUser();

            if (imageView.ImageFile == null || imageView.ImageFile.Length <= 0)
            {
                ViewBag.Message = "No image file specified!";
                return View(imageView);
            }

            _logger.LogDebug("....saving image metadata in the database....");

            string imageId = null;

            // TODO save image metadata in the database 
            
            // end TODO

            _logger.LogDebug("...saving image file on disk....");

            // TODO save image file on disk

            _logger.LogDebug("....forwarding to the details page, image Id = "+imageId);

            return RedirectToAction("Details", new { UserId = user.Id, Id = imageId });
        }

        // TODO

        public async Task<ActionResult> Details(string UserId, string Id)
        {
            CheckAda();

            var image = await ImageStorage.GetImageInfoAsync(UserId, Id);
            if (image == null)
            {
                return RedirectToAction("Error", "Home", new { ErrId = "Details: " + Id });
            }

            var imageView = new ImageView()
            {
                Id = image.Id,
                Caption = image.Caption,
                Description = image.Description,
                DateTaken = image.DateTaken,
                Uri = ImageStorage.ImageUri(image.UserId, image.Id),

                UserName = image.UserName,
                UserId = image.UserId
            };

            // TODO Log this view of the image


            return View(imageView);
        }

        // TODO

        public async Task<ActionResult> Edit(string UserId, string Id)
        {
            CheckAda();
            ApplicationUser user = await GetLoggedInUser();
            if (user == null || !user.Id.Equals(UserId))
            {
                return RedirectToAction("Error", "Home", new { ErrId = "EditNotAuth" });
            }

            Image image = await ImageStorage.GetImageInfoAsync(UserId, Id);
            if (image == null)
            {
                return RedirectToAction("Error", "Home", new { ErrId = "EditNotFound" });
            }

            ViewBag.Message = "";

            ImageView imageView = new ImageView()
            {
                Id = image.Id,
                Caption = image.Caption,
                Description = image.Description,
                DateTaken = image.DateTaken,

                UserId = image.UserId,
                UserName = image.UserName
            };

            return View("Edit", imageView);
        }

        // TODO prevent CSRF

        public async Task<ActionResult> DoEdit(string UserId, string Id, ImageView imageView)
        {
            CheckAda();

            if (!ModelState.IsValid)
            {
                ViewBag.Message = "Please correct the errors on the page";
                imageView.Id = Id;
                return View("Edit", imageView);
            }

            ApplicationUser user = await GetLoggedInUser();
            if (user == null || !user.Id.Equals(UserId))
            {
                return RedirectToAction("Error", "Home", new { ErrId = "EditNotAuth" });
            }

            _logger.LogDebug("Saving changes to image " + Id);
            Image image = await ImageStorage.GetImageInfoAsync(imageView.UserId, Id);
            if (image == null)
            {
                return RedirectToAction("Error", "Home", new { ErrId = "EditNotFound" });
            }

            image.Caption = imageView.Caption;
            image.Description = imageView.Description;
            image.DateTaken = imageView.DateTaken;
            await ImageStorage.UpdateImageInfoAsync(image);

            return RedirectToAction("Details", new { UserId = UserId, Id = Id });
        }

        // TODO

        public async Task<ActionResult> Delete(string UserId, string Id)
        {
            CheckAda();
            ApplicationUser user = await GetLoggedInUser();
            if (user == null || !user.Id.Equals(UserId))
            {
                return RedirectToAction("Error", "Home", new { ErrId = "EditNotAuth" });
            }

            Image image = await ImageStorage.GetImageInfoAsync(user.Id, Id);
            if (image == null)
            {
                return RedirectToAction("Error", "Home", new { ErrId = "EditNotFound" });
            }

            ImageView imageView = new ImageView();
            imageView.Id = image.Id;
            imageView.Caption = image.Caption;
            imageView.Description = image.Description;
            imageView.DateTaken = image.DateTaken;

            imageView.UserName = image.UserName;
            return View(imageView);
        }

        // TODO prevent CSRF

        public async Task<ActionResult> DoDelete(string UserId, string Id)
        {
            CheckAda();
            ApplicationUser user = await GetLoggedInUser();
            if (user == null || !user.Id.Equals(UserId))
            {
                return RedirectToAction("Error", "Home", new { ErrId = "EditNotAuth" });
            }

            Image image = await ImageStorage.GetImageInfoAsync(user.Id, Id);
            if (image == null)
            {
                return RedirectToAction("Error", "Home", new { ErrId = "EditNotFound" });
            }

            await ImageStorage.RemoveImageAsync(image);

            return RedirectToAction("Index", "Home");

        }

        // TODO
        public async Task<ActionResult> ListAll()
        {
            CheckAda();
            ApplicationUser user = await GetLoggedInUser();

            IList<Image> images = await ImageStorage.GetAllImagesInfoAsync();
            ViewBag.UserId = user.Id;
            return View(images);
        }

        // TODO
        public async Task<IActionResult> ListByUser()
        {
            CheckAda();

            // Return form for selecting a user from a drop-down list
            var userView = new ListByUserModel();
            var defaultId = (await GetLoggedInUser()).Id;

            userView.Users = new SelectList(ActiveUsers(), "Id", "UserName", defaultId);
            return View(userView);
        }

        // TODO
        public async Task<ActionResult> DoListByUser(ListByUserModel userView)
        {
            CheckAda();

            var user = await GetLoggedInUser();
            ViewBag.UserId = user.Id;

            var theUser = await UserManager.FindByIdAsync(userView.Id);
            if (theUser == null)
            {
                return RedirectToAction("Error", "Home", new { ErrId = "ListByUser" });
            }

            // TODO list all images uploaded by the user in userView
            /*
             * Eager loading of related entities
             */
            return View();
            // End TODO

        }

        // TODO

        public ActionResult ImageViews()
        {
            CheckAda();
            return View();
        }


        // TODO

        public ActionResult ImageViewsList(string Today)
        {
            CheckAda();
            _logger.LogDebug("Looking up log views, \"Today\"={today}", Today);
            AsyncPageable<LogEntry> entries = _logContext.Logs("true".Equals(Today));
            _logger.LogDebug("Query completed, rendering results....");
            return View(entries);
        }

    }

}
