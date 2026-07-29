using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Areas.Admin.Controllers;

// Admin-approval workflow for cleaning up tables that have grown very large, and for pausing
// customers who've gone quiet for a long time. Nothing here is automatic — the admin sees
// exactly what CAN be done and explicitly approves it.
[Authorize(Roles = "Admin")]
[Area("Admin")]
public class DataCleanupController : Controller
{
    private readonly IDataCleanupService _cleanupService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<DataCleanupController> _logger;

    public DataCleanupController(
        IDataCleanupService cleanupService,
        ICustomerService customerService,
        ILogger<DataCleanupController> logger)
    {
        _cleanupService = cleanupService;
        _customerService = customerService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DataCleanupPageViewModel
        {
            TableCleanup = await _cleanupService.GetOverviewAsync(),
            InactivityThresholdMonths = _customerService.InactivityThresholdMonths,
            InactiveCustomersCount = await _customerService.CountInactiveEligibleAsync()
        };

        if (model.InactiveCustomersCount > 0)
        {
            // Small preview list so the admin can see WHO before approving, without loading
            // potentially thousands of rows if the count is large.
            model.InactiveCustomersPreview = (await _customerService.GetInactiveEligibleAsync())
                .Take(15)
                .ToList();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string key, int count)
    {
        try
        {
            var deleted = await _cleanupService.DeleteAsync(key, count);
            TempData["SuccessMessage"] = deleted > 0
                ? $"تمت الموافقة على العملية وتم حذف {deleted:N0} عنصر بنجاح."
                : "لم يتم حذف أي عنصر.";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during admin-approved data cleanup.");
            TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء عملية الحذف.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateInactiveCustomers()
    {
        try
        {
            var count = await _customerService.DeactivateInactiveAsync();
            TempData["SuccessMessage"] = count > 0
                ? $"تم إيقاف {count:N0} عميل غير نشط بنجاح. يمكن إعادة تفعيل أي عميل في أي وقت من صفحة تفاصيله."
                : "لا يوجد عملاء غير نشطين حاليًا لإيقافهم.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while pausing inactive customers.");
            TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء العملية.";
        }

        return RedirectToAction(nameof(Index));
    }
}
