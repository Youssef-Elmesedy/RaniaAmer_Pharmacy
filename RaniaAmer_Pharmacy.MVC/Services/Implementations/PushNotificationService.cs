using System.Net;
using System.Text.Json;
using RaniaAmer_Pharmacy.MVC.Data;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using WebPush;
using MyPushSubscription = RaniaAmer_Pharmacy.MVC.Models.Entities.PushSubscription;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

public class PushNotificationService : IPushNotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PushNotificationService> _logger;
    private readonly string _publicKey;
    private readonly VapidDetails _vapidDetails;

    public PushNotificationService(ApplicationDbContext context, IConfiguration configuration, ILogger<PushNotificationService> logger)
    {
        _context = context;
        _logger = logger;

        _publicKey = configuration["WebPush:PublicKey"] ?? string.Empty;
        var privateKey = configuration["WebPush:PrivateKey"] ?? string.Empty;
        var subject = configuration["WebPush:Subject"] ?? "mailto:admin@example.com";

        _vapidDetails = new VapidDetails(subject, _publicKey, privateKey);
    }

    public string GetPublicKey() => _publicKey;

    public async Task SaveCustomerSubscriptionAsync(Guid customerId, string endpoint, string p256dh, string auth)
    {
        await ReplaceExistingAsync(endpoint);

        _context.PushSubscriptions.Add(MyPushSubscription.ForCustomer(customerId, endpoint, p256dh, auth));
        await _context.SaveChangesAsync();
    }

    public async Task SaveAdminSubscriptionAsync(string adminUserId, string endpoint, string p256dh, string auth)
    {
        await ReplaceExistingAsync(endpoint);

        _context.PushSubscriptions.Add(MyPushSubscription.ForAdmin(adminUserId, endpoint, p256dh, auth));
        await _context.SaveChangesAsync();
    }

    public async Task RemoveSubscriptionAsync(string endpoint)
    {
        var existing = await _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing == null) return;

        _context.PushSubscriptions.Remove(existing);
        await _context.SaveChangesAsync();
    }

    public async Task SendToCustomerAsync(Guid customerId, string title, string body, string? url = null)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(s => s.CustomerId == customerId)
            .ToListAsync();

        await SendToAllAsync(subscriptions, title, body, url);
    }

    public async Task SendToAllAdminsAsync(string title, string body, string? url = null)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(s => s.AdminUserId != null)
            .ToListAsync();

        await SendToAllAsync(subscriptions, title, body, url);
    }

    public async Task SendToAllCustomersAsync(string title, string body, string? url = null)
    {
        var subscriptions = await _context.PushSubscriptions
            .Where(s => s.CustomerId != null)
            .ToListAsync();

        await SendToAllAsync(subscriptions, title, body, url);
    }

    // Re-subscribing on the same browser arrives with the same Endpoint but possibly refreshed
    // keys - replace rather than duplicate.
    private async Task ReplaceExistingAsync(string endpoint)
    {
        var existing = await _context.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing == null) return;

        _context.PushSubscriptions.Remove(existing);
        await _context.SaveChangesAsync();
    }

    private async Task SendToAllAsync(List<MyPushSubscription> subscriptions, string title, string body, string? url)
    {
        if (subscriptions.Count == 0) return;

        if (string.IsNullOrEmpty(_vapidDetails.PublicKey) || string.IsNullOrEmpty(_vapidDetails.PrivateKey))
        {
            _logger.LogWarning("Push notification skipped: WebPush VAPID keys are not configured (see appsettings.json -> WebPush).");
            return;
        }

        var client = new WebPushClient();
        var payload = JsonSerializer.Serialize(new
        {
            title,
            body,
            url = string.IsNullOrWhiteSpace(url) ? "/" : url
        });

        var expiredEndpoints = new List<string>();

        foreach (var sub in subscriptions)
        {
            try
            {
                var pushSubscription = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                await client.SendNotificationAsync(pushSubscription, payload, _vapidDetails);
            }
            catch (WebPushException ex) when (ex.StatusCode == HttpStatusCode.NotFound || ex.StatusCode == HttpStatusCode.Gone)
            {
                // The browser unsubscribed, or the subscription expired - stop trying it.
                expiredEndpoints.Add(sub.Endpoint);
            }
            catch (Exception ex)
            {
                // A single failed device should never stop the others, or break whatever
                // business action (new order, order completed...) triggered this notification.
                _logger.LogWarning(ex, "Failed to send a push notification to one subscription.");
            }
        }

        if (expiredEndpoints.Count > 0)
        {
            var toRemove = await _context.PushSubscriptions
                .Where(s => expiredEndpoints.Contains(s.Endpoint))
                .ToListAsync();

            _context.PushSubscriptions.RemoveRange(toRemove);
            await _context.SaveChangesAsync();
        }
    }
}
