using RaniaAmer_Pharmacy.MVC.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace RaniaAmer_Pharmacy.MVC.Hubs;

/// <summary>
/// Real-time companion to the Web Push notifications (see IPushNotificationService): push
/// reaches a person even when the site isn't open at all, this hub gives an INSTANT heads-up
/// while they already have a tab open, with no page reload needed.
///
/// Accepts connections from either auth scheme this app has (admin via ASP.NET Core Identity,
/// customer via the separate CustomerScheme cookie) and sorts each connection into the right
/// group so notifications only reach the people they're meant for.
/// </summary>
[Authorize(AuthenticationSchemes = AuthSchemes.AdminOrCustomer)]
public class NotificationHub : Hub
{
    public const string AdminsGroup = "Admins";
    public const string AllCustomersGroup = "AllCustomers";

    public static string CustomerGroup(Guid customerId) => $"customer-{customerId}";

    public override async Task OnConnectedAsync()
    {
        if (Context.User?.IsInRole("Admin") == true)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, AdminsGroup);
        }
        else
        {
            var customerIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(customerIdClaim, out var customerId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, CustomerGroup(customerId));
                await Groups.AddToGroupAsync(Context.ConnectionId, AllCustomersGroup);
            }
        }

        await base.OnConnectedAsync();
    }
}
