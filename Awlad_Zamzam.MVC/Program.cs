using Awlad_Zamzam.MVC.Data;
using Awlad_Zamzam.MVC.Models.Entities;
using Awlad_Zamzam.MVC.Repository.Implementations;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Implementations;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Awlad_Zamzam.MVC
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Configure antiforgery options (custom cookie name and header name)
            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "AwladZamzam.AntiForgery";
                options.HeaderName = "X-CSRF-TOKEN";
            });

            // Add DbContext and Identity services
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"));
            });

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = true;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 6;

                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                    options.Lockout.AllowedForNewUsers = true;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/Login";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            });

            // Separate cookie scheme for customer logins (independent from the Admin Identity system)
            builder.Services.AddAuthentication()
            .AddCookie(Controllers.CustomerAccountController.SchemeName, options =>
            {
                options.LoginPath = "/CustomerAccount/Login";
                options.AccessDeniedPath = "/CustomerAccount/Login";

                options.Cookie.Name = "CustomerAuth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            });

            // In-memory caching (category list, active offers, pending-orders count)
            builder.Services.AddMemoryCache();

            // Shared "catalog version" counter so public pages can detect admin changes and auto-refresh
            builder.Services.AddSingleton<ICatalogChangeTracker, CatalogChangeTracker>();

            // Session (used for the guest shopping cart - there is no customer login)
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(6);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Repositories
            builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IOfferRepository, OfferRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            // Services
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IOfferService, OfferService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Only route actual 404s to the branded "not found" page. Previously ALL
            // error status codes (400/401/403/etc.) were re-executed to /Home/Error404,
            // which made unrelated problems (e.g. a stale antiforgery token from a
            // browser back/forward-cache restore) confusingly look like "page not found".
            app.UseStatusCodePages(context =>
            {
                if (context.HttpContext.Response.StatusCode == 404)
                {
                    context.HttpContext.Response.Redirect("/Home/Error404");
                }

                return Task.CompletedTask;
            });

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            // Configure cookie policy to ensure cookies are secure and have appropriate SameSite settings
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                Secure = CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

            app.UseRouting();

            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Apply pending migrations and seed the admin user + default categories
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var context = services.GetRequiredService<ApplicationDbContext>();
                await context.Database.MigrateAsync();
                await SeedData.SeedAsync(services);
            }

            app.Run();
        }
    }
}
