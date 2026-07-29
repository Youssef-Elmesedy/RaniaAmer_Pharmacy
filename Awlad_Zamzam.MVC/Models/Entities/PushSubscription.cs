namespace Awlad_Zamzam.MVC.Models.Entities;

// One browser/device subscription for Web Push notifications. Belongs to EITHER a Customer OR
// an admin (ApplicationUser) — never both. A single person can have several rows here (one per
// browser/device they granted notification permission on).
public class PushSubscription : BaseEntity
{
    public string Endpoint { get; private set; } = string.Empty;
    public string P256dh { get; private set; } = string.Empty;
    public string Auth { get; private set; } = string.Empty;

    public Guid? CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    public string? AdminUserId { get; private set; }

    private PushSubscription()
    {
    }

    public static PushSubscription ForCustomer(Guid customerId, string endpoint, string p256dh, string auth)
    {
        Validate(endpoint, p256dh, auth);

        return new PushSubscription
        {
            CustomerId = customerId,
            Endpoint = endpoint,
            P256dh = p256dh,
            Auth = auth,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static PushSubscription ForAdmin(string adminUserId, string endpoint, string p256dh, string auth)
    {
        if (string.IsNullOrWhiteSpace(adminUserId))
            throw new ArgumentException("Admin user id is required", nameof(adminUserId));

        Validate(endpoint, p256dh, auth);

        return new PushSubscription
        {
            AdminUserId = adminUserId,
            Endpoint = endpoint,
            P256dh = p256dh,
            Auth = auth,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void Validate(string endpoint, string p256dh, string auth)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            throw new ArgumentException("Endpoint is required", nameof(endpoint));

        if (string.IsNullOrWhiteSpace(p256dh))
            throw new ArgumentException("p256dh key is required", nameof(p256dh));

        if (string.IsNullOrWhiteSpace(auth))
            throw new ArgumentException("auth key is required", nameof(auth));
    }
}
