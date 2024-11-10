using System;
using System.Threading.Tasks;
using ImageSharingWithSecurity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ImageSharingWithSecurity.DAL
{
    public  class ApplicationDbInitializer
    {
        private ApplicationDbContext db;
        private ILogger<ApplicationDbInitializer> logger;
        public ApplicationDbInitializer(ApplicationDbContext db, ILogger<ApplicationDbInitializer> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public async Task SeedDatabase(IServiceProvider serviceProvider)
        {

            await db.Database.MigrateAsync();

            db.RemoveRange(db.Images);
            db.RemoveRange(db.Tags);
            db.RemoveRange(db.Users);
            await db.SaveChangesAsync();

            logger.LogDebug("Adding role: User");
            var idResult = await CreateRole(serviceProvider, "User");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create User role!");
            }

            // TODO add other roles
            logger.LogDebug("Adding role: Approver");
            idResult = await CreateRole(serviceProvider, "Approver");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create Approver role!");
            }

            logger.LogDebug("Adding role: Admin");
            idResult = await CreateRole(serviceProvider, "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create Admin role!");
            }

            logger.LogDebug("Adding admin: jfk");
            idResult = await CreateAccount(serviceProvider, "jfk@example.org", "Jfk123@@", "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create jfk user!");
            }

            logger.LogDebug("Adding admin: mahesh");
            idResult = await CreateAccount(serviceProvider, "mahesh@example.org", "Mahesh123@@", "Admin");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create mahesh user!");
            }

            logger.LogDebug("Adding approver: nixon");
            idResult = await CreateAccount(serviceProvider, "nixon@example.org", "Nixon123@@", "Approver");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create nixon user!");
            }

            // TODO add other users and assign more roles
            logger.LogDebug("Adding approver: lincoln");
            idResult = await CreateAccount(serviceProvider, "lincoln@example.org", "Lincoln123@@", "Approver");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create lincoln user!");
            }

            logger.LogDebug("Adding user: washington");
            idResult = await CreateAccount(serviceProvider, "washington@example.org", "Washington123@@", "User");
            if (!idResult.Succeeded)
            {
                logger.LogDebug("Failed to create washington user!");
            }


            Tag portrait = new Tag { Name = "portrait" };
            await db.Tags.AddAsync(portrait);
            Tag architecture = new Tag { Name = "architecture" };
            await db.Tags.AddAsync(architecture);

            // TODO add other tags
            Tag landscape = new Tag { Name = "landscape" };
            await db.Tags.AddAsync(landscape);
            Tag wildlife = new Tag { Name = "wildlife" };
            await db.Tags.AddAsync(wildlife);

            await db.SaveChangesAsync();

        }

        private static async Task<IdentityResult> CreateRole(IServiceProvider provider,
                                                            string role)
        {
            RoleManager<IdentityRole> roleManager = provider
                .GetRequiredService
                       <RoleManager<IdentityRole>>();
            var idResult = IdentityResult.Success;
            if (await roleManager.FindByNameAsync(role) == null)
            {
                idResult = await roleManager.CreateAsync(new IdentityRole(role));
            }
            return idResult;
        }

        private static async Task<IdentityResult> CreateAccount(IServiceProvider provider,
                                                               string email, 
                                                               string password,
                                                               string role)
        {
            UserManager<ApplicationUser> userManager = provider
                .GetRequiredService
                       <UserManager<ApplicationUser>>();
            var idResult = IdentityResult.Success;

            if (await userManager.FindByNameAsync(email) == null)
            {
                ApplicationUser user = new ApplicationUser { UserName = email, Email = email };
                idResult = await userManager.CreateAsync(user, password);

                if (idResult.Succeeded)
                {
                    idResult = await userManager.AddToRoleAsync(user, role);
                } else {
                    foreach (var error in idResult.Errors)
                    {
                        provider.GetRequiredService<ILogger<ApplicationDbInitializer>>().LogError($"Error creating user {email}: {error.Description}");
                    }
                }
            }

            return idResult;
        }

    }
}