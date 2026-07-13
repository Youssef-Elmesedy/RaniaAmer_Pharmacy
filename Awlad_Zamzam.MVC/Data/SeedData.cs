using Awlad_Zamzam.MVC.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace Awlad_Zamzam.MVC.Data;

public static class SeedData
{
    private const string AdminRole = "Admin";
    private const string AdminEmail = "admin@awladzamzam.com";
    private const string AdminPassword = "Admin@123";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var context = services.GetRequiredService<ApplicationDbContext>();

        if (!await roleManager.RoleExistsAsync(AdminRole))
            await roleManager.CreateAsync(new IdentityRole(AdminRole));

        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                FullName = "مدير المحل",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, AdminRole);
        }

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                Category.Create("اللحوم النيئة", "/images/seed/fresh-meat.svg"),
                Category.Create("مصنعات اللحوم", "/images/seed/processed-meat.svg"),
                Category.Create("المشويات", "/images/seed/grills.svg")
            );

            await context.SaveChangesAsync();
        }
    }
}
