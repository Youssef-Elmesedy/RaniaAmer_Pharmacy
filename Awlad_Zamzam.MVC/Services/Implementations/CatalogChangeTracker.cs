using Awlad_Zamzam.MVC.Services.Interfaces;

namespace Awlad_Zamzam.MVC.Services.Implementations;

// Registered as a Singleton so the same counter is shared across every request.
public class CatalogChangeTracker : ICatalogChangeTracker
{
    private long _version = DateTime.UtcNow.Ticks;

    public void Touch() => Interlocked.Exchange(ref _version, DateTime.UtcNow.Ticks);

    public long GetVersion() => Interlocked.Read(ref _version);
}
