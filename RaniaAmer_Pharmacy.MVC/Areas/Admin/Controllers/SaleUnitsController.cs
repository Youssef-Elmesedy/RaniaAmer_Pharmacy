using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaniaAmer_Pharmacy.MVC.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class SaleUnitsController : Controller
{
    private readonly ISaleUnitService _saleUnitService;
    private readonly ILogger<SaleUnitsController> _logger;

    public SaleUnitsController(ISaleUnitService saleUnitService, ILogger<SaleUnitsController> logger)
    {
        _saleUnitService = saleUnitService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var units = await _saleUnitService.GetAllAsync();
        return View(units.OrderBy(u => u.Name).ToList());
    }

    public IActionResult Create() => View(new SaleUnitFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaleUnitFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _saleUnitService.CreateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating a sale unit.");
            ModelState.AddModelError(string.Empty,
                "حدث خطأ غير متوقع أثناء الحفظ. تأكد من اتصال قاعدة البيانات وحاول مرة أخرى.");
            return View(model);
        }

        TempData["SuccessMessage"] = "تم إضافة وحدة البيع بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await _saleUnitService.GetForEditAsync(id);
        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SaleUnitFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _saleUnitService.UpdateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating a sale unit.");
            ModelState.AddModelError(string.Empty,
                "حدث خطأ غير متوقع أثناء الحفظ. تأكد من اتصال قاعدة البيانات وحاول مرة أخرى.");
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تعديل وحدة البيع بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _saleUnitService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف وحدة البيع بنجاح";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting a sale unit.");
            TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء الحذف.";
        }

        return RedirectToAction(nameof(Index));
    }
}
