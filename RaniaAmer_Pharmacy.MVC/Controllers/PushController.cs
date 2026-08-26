using RaniaAmer_Pharmacy.MVC.Common;
using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RaniaAmer_Pharmacy.MVC.Controllers;

[Route("push")]
[ApiController]
public class PushController : ControllerBase
{
    private readonly IPushNotificationService _pushService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<PushController> _logger;

    public PushController(
        IPushNotificationService pushService,
        UserManager<ApplicationUser> userManager,
        ILogger<PushController> logger)
    {
        _pushService = pushService;
        _userManager = userManager;
        _logger = logger;
    }

    // Public - the browser needs this key before it can even ask for a push subscription
    [HttpGet("vapid-public-key")]
    [AllowAnonymous]
    public IActionResult GetVapidPublicKey() => Ok(new { publicKey = _pushService.GetPublicKey() });

    [HttpPost("subscribe/customer")]
    [Authorize(AuthenticationSchemes = CustomerAccountController.SchemeName)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubscribeCustomer([FromBody] PushSubscriptionDto dto)
    {
        if (!TryValidate(dto, out var error))
            return BadRequest(new { error });

        var customerId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {
            await _pushService.SaveCustomerSubscriptionAsync(customerId, dto.Endpoint, dto.Keys!.P256dh, dto.Keys!.Auth);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save a customer push subscription.");
            return StatusCode(500, new { error = "تعذر حفظ الاشتراك في الإشعارات." });
        }
    }

    [HttpPost("subscribe/admin")]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubscribeAdmin([FromBody] PushSubscriptionDto dto)
    {
        if (!TryValidate(dto, out var error))
            return BadRequest(new { error });

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        try
        {
            await _pushService.SaveAdminSubscriptionAsync(user.Id, dto.Endpoint, dto.Keys!.P256dh, dto.Keys!.Auth);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save an admin push subscription.");
            return StatusCode(500, new { error = "تعذر حفظ الاشتراك في الإشعارات." });
        }
    }

    // Shared by both customer and admin pages - accept either auth scheme (this app has two:
    // ASP.NET Core Identity for admins, and a separate cookie scheme for customers)
    [HttpPost("unsubscribe")]
    [Authorize(AuthenticationSchemes = AuthSchemes.AdminOrCustomer)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unsubscribe([FromBody] UnsubscribeDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Endpoint))
            return BadRequest(new { error = "endpoint مطلوب" });

        await _pushService.RemoveSubscriptionAsync(dto.Endpoint);
        return Ok(new { success = true });
    }

    private static bool TryValidate(PushSubscriptionDto? dto, out string error)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Endpoint) || dto.Keys == null ||
            string.IsNullOrWhiteSpace(dto.Keys.P256dh) || string.IsNullOrWhiteSpace(dto.Keys.Auth))
        {
            error = "بيانات الاشتراك غير مكتملة.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}

public class PushSubscriptionDto
{
    public string Endpoint { get; set; } = string.Empty;
    public PushSubscriptionKeysDto? Keys { get; set; }
}

public class PushSubscriptionKeysDto
{
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}

public class UnsubscribeDto
{
    public string Endpoint { get; set; } = string.Empty;
}
