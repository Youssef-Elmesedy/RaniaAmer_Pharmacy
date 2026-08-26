using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaniaAmer_Pharmacy.MVC.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class BranchesController : Controller
{
    private readonly IBranchService _branchService;
    private readonly ILogger<BranchesController> _logger;

    public BranchesController(IBranchService branchService, ILogger<BranchesController> logger)
    {
        _branchService = branchService;
        _logger = logger;
    }

    public async Task<IActionResult> Index() => View(await _branchService.GetAllAsync());

    public IActionResult Create() => View(new BranchFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BranchFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _branchService.CreateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating a branch.");
            ModelState.AddModelError(string.Empty,
                "حدث خطأ غير متوقع أثناء الحفظ. تأكد من اتصال قاعدة البيانات وحاول مرة أخرى.");
            return View(model);
        }

        TempData["SuccessMessage"] = "تم إضافة الفرع بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await _branchService.GetForEditAsync(id);
        if (model == null) return NotFound();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BranchFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _branchService.UpdateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating a branch.");
            ModelState.AddModelError(string.Empty,
                "حدث خطأ غير متوقع أثناء الحفظ. تأكد من اتصال قاعدة البيانات وحاول مرة أخرى.");
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تعديل بيانات الفرع بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _branchService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف الفرع بنجاح";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting a branch.");
            TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء الحذف.";
        }

        return RedirectToAction(nameof(Index));
    }
}
