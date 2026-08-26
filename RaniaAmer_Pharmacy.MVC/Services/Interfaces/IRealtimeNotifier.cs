namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

/// <summary>
/// Instant in-tab notifications over SignalR - the live companion to IPushNotificationService.
/// Push reaches a person even with the site closed; this reaches them instantly while a tab is
/// already open, no page reload needed. The two are normally called together for the same event.
/// </summary>
public interface IRealtimeNotifier
{
    Task NotifyAdminsAsync(string title, string body, string? url = null);

    Task NotifyCustomerAsync(Guid customerId, string title, string body, string? url = null);

    Task NotifyAllCustomersAsync(string title, string body, string? url = null);
}
