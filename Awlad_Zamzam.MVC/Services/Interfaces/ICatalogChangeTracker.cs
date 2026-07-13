namespace Awlad_Zamzam.MVC.Services.Interfaces;

// Tracks a single "version" that changes whenever the public catalog (products, categories,
// offers/discounts) is modified by the admin. Public pages poll this so customers see new
// content without needing to manually refresh.
public interface ICatalogChangeTracker
{
    void Touch();
    long GetVersion();
}
