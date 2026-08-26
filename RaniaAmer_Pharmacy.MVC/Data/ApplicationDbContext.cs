using RaniaAmer_Pharmacy.MVC.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace RaniaAmer_Pharmacy.MVC.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<SaleUnit> SaleUnits => Set<SaleUnit>();

    public DbSet<ProductUnitOption> ProductUnitOptions => Set<ProductUnitOption>();

    public DbSet<SiteSettings> SiteSettings => Set<SiteSettings>();

    public DbSet<Branch> Branches => Set<Branch>();

    public DbSet<Customer> Customers => Set<Customer>();

    public DbSet<Offer> Offers => Set<Offer>();

    public DbSet<OfferItem> OfferItems => Set<OfferItem>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<OrderPayment> OrderPayments => Set<OrderPayment>();

    public DbSet<Models.Entities.PushSubscription> PushSubscriptions => Set<Models.Entities.PushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}