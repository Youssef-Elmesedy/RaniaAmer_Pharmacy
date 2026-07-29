namespace Awlad_Zamzam.MVC.Services.Interfaces;

public interface IPushNotificationService
{
    /// <summary>The VAPID public key the browser needs to create a push subscription.</summary>
    string GetPublicKey();

    Task SaveCustomerSubscriptionAsync(Guid customerId, string endpoint, string p256dh, string auth);

    Task SaveAdminSubscriptionAsync(string adminUserId, string endpoint, string p256dh, string auth);

    Task RemoveSubscriptionAsync(string endpoint);

    /// <summary>Sends a notification to every browser/device this customer subscribed on.</summary>
    Task SendToCustomerAsync(Guid customerId, string title, string body, string? url = null);

    /// <summary>Sends a notification to every browser/device every admin subscribed on.</summary>
    Task SendToAllAdminsAsync(string title, string body, string? url = null);
}
