using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RaniaAmer_Pharmacy.MVC.Components.PendingOrdersBadge;

// Small badge shown in the admin sidebar next to "Notifications" / "Orders".
// Rendered on EVERY admin page (it lives in _AdminLayout), so it must never let a transient
// DB error take down an otherwise-unrelated page (e.g. Products/Create).
public class PendingOrdersBadgeViewComponent : ViewComponent
{
    private readonly IOrderService _orderService;
    private readonly ILogger<PendingOrdersBadgeViewComponent> _logger;

    public PendingOrdersBadgeViewComponent(IOrderService orderService, ILogger<PendingOrdersBadgeViewComponent> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = 0;

        try
        {
            count = await _orderService.GetPendingCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load pending orders count for the admin badge.");
        }

        return View(count);
    }
}
