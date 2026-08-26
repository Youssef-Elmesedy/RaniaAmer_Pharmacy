using Microsoft.AspNetCore.Identity;
using RaniaAmer_Pharmacy.MVC.Models.Entities;

namespace RaniaAmer_Pharmacy.MVC.Data;

public static class SeedData
{
    private const string AdminRole = "Admin";
    private const string AdminEmail = "admin@raniaamerpharmacy.com";
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
                FullName = "D/Ranai Amer",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, AdminRole);
        }

        if (!context.Categories.Any())
        {
            context.Categories.AddRange(
                Category.Create("الأدوية", "/images/seed/medicines.svg"),
                Category.Create("العناية بالبشرة والشعر", "/images/seed/skincare.svg"),
                Category.Create("الفيتامينات والمكملات الغذائية", "/images/seed/vitamins.svg"),
                Category.Create("الأمومة والطفل", "/images/seed/baby-care.svg"),
                Category.Create("الأجهزة والمستلزمات الطبية", "/images/seed/medical-devices.svg")
            );

            await context.SaveChangesAsync();
        }

        if (!context.SaleUnits.Any())
        {
            context.SaleUnits.AddRange(
                SaleUnit.Create("قطعة"),
                SaleUnit.Create("علبة"),
                SaleUnit.Create("شريط"),
                SaleUnit.Create("قرص"),
                SaleUnit.Create("زجاجة"),
                SaleUnit.Create("أنبوبة"),
                SaleUnit.Create("سرنجة"),
                SaleUnit.Create("كيس")
            );

            await context.SaveChangesAsync();
        }

        if (!context.SiteSettings.Any())
        {
            context.SiteSettings.Add(SiteSettings.CreateDefault());

            await context.SaveChangesAsync();
        }

        if (!context.Branches.Any())
        {
            context.Branches.Add(Branch.Create(
                name: "الفرع الرئيسي",
                phoneNumber: "01000000000",
                address: "[اسم الشارع] - [المدينة - المحافظة]",
                workingHours: "يوميًا 9 صباحًا - 1 مساءً",
                deliveryAreaText: null,
                mapEmbedUrl: null,
                mapDirectionsUrl: null,
                displayOrder: 0));

            await context.SaveChangesAsync();
        }
    }
}
