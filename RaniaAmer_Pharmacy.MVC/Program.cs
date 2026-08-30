using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Repository.Implementations;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Implementations;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Gzip/Brotli compression for smaller page payloads (bigger win on mobile connections)
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
            });
            builder.Services.AddSignalR();

            // Persist Data Protection keys to disk
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys")))
                .SetApplicationName("RaniaAmerPharmacyMVC");

            // Configure antiforgery options
            builder.Services.AddAntiforgery(options =>
            {
                options.Cookie.Name = "RaniaAmerPharmacy.AntiForgery";
                options.HeaderName = "X-CSRF-TOKEN";
            });

            // Add DbContext and Identity services
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions => sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null));
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

            // Separate cookie scheme for customer logins
            builder.Services.AddAuthentication()
            .AddCookie(Controllers.CustomerAccountController.SchemeName, options =>
            {
                options.LoginPath = "/CustomerAccount/Login";
                options.AccessDeniedPath = "/CustomerAccount/Login";

                options.Cookie.Name = "CustomerAuth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                                             ? CookieSecurePolicy.SameAsRequest
                                             : CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;

                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.SlidingExpiration = true;
            });

            // Memory caching
            builder.Services.AddMemoryCache();

            // Catalog change tracker
            builder.Services.AddSingleton<ICatalogChangeTracker, CatalogChangeTracker>();

            // Session
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
            builder.Services.AddScoped<ISaleUnitRepository, SaleUnitRepository>();
            builder.Services.AddScoped<ISiteSettingsRepository, SiteSettingsRepository>();
            builder.Services.AddScoped<IBranchRepository, BranchRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IOfferRepository, OfferRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            // Services
            builder.Services.AddScoped<IImageService, ImageService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<ISaleUnitService, SaleUnitService>();
            builder.Services.AddScoped<ISiteSettingsService, SiteSettingsService>();
            builder.Services.AddScoped<IBranchService, BranchService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IOfferService, OfferService>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddScoped<IOrderService, OrderService>();
            builder.Services.AddScoped<ICustomerAuthService, CustomerAuthService>();

            // Generic data-retention engine (counts/deletes oldest rows of any table) + the
            // admin-facing layer on top of it — nothing is ever deleted automatically; the
            // admin reviews and approves every cleanup from the "تنظيف البيانات" admin page.
            builder.Services.AddScoped<IDataRetentionService, DataRetentionService>();
            builder.Services.AddScoped<IDataCleanupService, DataCleanupService>();
            builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();
            builder.Services.AddScoped<IRealtimeNotifier, SignalRRealtimeNotifier>();

            var app = builder.Build();

            // Must be one of the first middlewares in the pipeline to compress everything downstream.
            app.UseResponseCompression();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            // Route ONLY 404 to the custom error page (Removed the status 400 redirect to root)
            app.UseStatusCodePages(context =>
            {
                var response = context.HttpContext.Response;

                if (response.StatusCode == 404)
                {
                    response.Redirect("/Home/Error404");
                }

                return Task.CompletedTask;
            });

            app.UseHttpsRedirection();

            // Standard hardening headers - cheap to add, meaningful protection against
            // clickjacking and MIME-type sniffing attacks.
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                await next();
            });

            app.UseStaticFiles();

            app.UseCookiePolicy(new CookiePolicyOptions
            {
                // السماح للـ Cookies بالعمل حسب البروتوكول الحالي (HTTP أو HTTPS)
                Secure = app.Environment.IsDevelopment()
                         ? CookieSecurePolicy.SameAsRequest
                         : CookieSecurePolicy.Always,
                MinimumSameSitePolicy = SameSiteMode.Lax
            });

            app.UseRouting();
            app.UseSession();

            // Prevent browser bfcache storage for secure views
            app.Use(async (context, next) =>
            {
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<RaniaAmer_Pharmacy.MVC.Hubs.NotificationHub>("/hubs/notifications");

            // Apply pending migrations and seed data
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