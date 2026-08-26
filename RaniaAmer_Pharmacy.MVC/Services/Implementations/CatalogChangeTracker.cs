using RaniaAmer_Pharmacy.MVC.Services.Interfaces;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

// Registered as a Singleton so the same counter is shared across every request.
public class CatalogChangeTracker : ICatalogChangeTracker
{
    private long _version = DateTime.UtcNow.Ticks;

    public void Touch() => Interlocked.Exchange(ref _version, DateTime.UtcNow.Ticks);

    public long GetVersion() => Interlocked.Read(ref _version);
}
