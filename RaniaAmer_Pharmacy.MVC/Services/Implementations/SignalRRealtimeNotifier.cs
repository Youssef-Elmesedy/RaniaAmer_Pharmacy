using RaniaAmer_Pharmacy.MVC.Hubs;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

public class SignalRRealtimeNotifier : IRealtimeNotifier
{
    private readonly IHubContext<NotificationHub> _hub;

    public SignalRRealtimeNotifier(IHubContext<NotificationHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyAdminsAsync(string title, string body, string? url = null) =>
        _hub.Clients.Group(NotificationHub.AdminsGroup)
            .SendAsync("ReceiveNotification", new { title, body, url });

    public Task NotifyCustomerAsync(Guid customerId, string title, string body, string? url = null) =>
        _hub.Clients.Group(NotificationHub.CustomerGroup(customerId))
            .SendAsync("ReceiveNotification", new { title, body, url });

    public Task NotifyAllCustomersAsync(string title, string body, string? url = null) =>
        _hub.Clients.Group(NotificationHub.AllCustomersGroup)
            .SendAsync("ReceiveNotification", new { title, body, url });
}
