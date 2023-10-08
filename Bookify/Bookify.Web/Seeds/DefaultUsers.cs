using Bookify.Web.Core.Consts;
using Bookify.Web.Core.Models;
using Microsoft.AspNetCore.Identity;

namespace Bookify.Web.Seeds
{
    public static class DefaultUsers
    {
        public static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            ApplicationUser admin = new()
            {
                UserName = "admin",
                Email = "admin@bookify.com",
                FullName = "Admin",
                EmailConfirmed = true
            };

            var user = await userManager.FindByNameAsync(admin.UserName);

            if(user is null)
            {
                await userManager.CreateAsync(admin, "P@ssword123");
                await userManager.AddToRoleAsync(admin, AppRoles.Admin);
            }
        }
    }
}