using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(
        ICustomerService customerService,
        IOrderService orderService,
        ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _orderService = orderService;
        _logger = logger;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var customers = await _customerService.SearchAsync(search);
        ViewBag.SearchTerm = search;
        return View(customers);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null) return NotFound();

        ViewBag.Orders = await _orderService.GetByCustomerAsync(id);
        return View(customer);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _customerService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف العميل بنجاح";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting a customer.");
            TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء الحذف.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reactivate(Guid id)
    {
        try
        {
            await _customerService.ReactivateAsync(id);
            TempData["SuccessMessage"] = "تم إعادة تفعيل حساب العميل بنجاح";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while reactivating a customer.");
            TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء العملية.";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
