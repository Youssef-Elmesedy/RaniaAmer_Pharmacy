using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RaniaAmer_Pharmacy.MVC.Components.DataCleanupBadge;

// Small badge shown in the admin sidebar next to "تنظيف البيانات".
// Rendered on EVERY admin page (it lives in _AdminLayout), so it must never let a transient
// DB error take down an otherwise-unrelated page.
public class DataCleanupBadgeViewComponent : ViewComponent
{
    private readonly IDataCleanupService _cleanupService;
    private readonly ICustomerService _customerService;
    private readonly ILogger<DataCleanupBadgeViewComponent> _logger;

    public DataCleanupBadgeViewComponent(
        IDataCleanupService cleanupService,
        ICustomerService customerService,
        ILogger<DataCleanupBadgeViewComponent> logger)
    {
        _cleanupService = cleanupService;
        _customerService = customerService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = 0;

        try
        {
            var overview = await _cleanupService.GetOverviewAsync();
            count = overview.Count;

            if (await _customerService.CountInactiveEligibleAsync() > 0)
                count += 1;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load the data-cleanup overview for the admin badge.");
        }

        return View(count);
    }
}
