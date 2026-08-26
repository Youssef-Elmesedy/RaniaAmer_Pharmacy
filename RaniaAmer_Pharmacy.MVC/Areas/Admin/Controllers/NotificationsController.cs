using RaniaAmer_Pharmacy.MVC.Models.Enums;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaniaAmer_Pharmacy.MVC.Areas.Admin.Controllers;

// Shows pending (not-yet-handled) orders so the admin doesn't miss a new "طلب"
[Authorize(Roles = "Admin")]
[Area("Admin")]
public class NotificationsController : Controller
{
    private readonly IOrderService _orderService;

    public NotificationsController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetAllAsync();
        var pending = orders
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        return View(pending);
    }

    // Polled periodically from the admin layout to detect new orders without a manual refresh
    [HttpGet]
    public async Task<IActionResult> PendingCount()
    {
        var count = await _orderService.GetPendingCountAsync();
        return Json(new { count });
    }
}
