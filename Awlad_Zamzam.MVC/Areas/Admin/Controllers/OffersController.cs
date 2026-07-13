using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class OffersController : Controller
{
    private readonly IOfferService _offerService;

    public OffersController(IOfferService offerService)
    {
        _offerService = offerService;
    }

    public async Task<IActionResult> Index()
    {
        var offers = await _offerService.GetAllForAdminAsync();
        return View(offers);
    }

    public async Task<IActionResult> Create()
    {
        var model = await _offerService.GetForCreateAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OfferFormViewModel model)
    {
        BindSpecialPrices(model);

        if (!ModelState.IsValid)
        {
            model.AllProducts = (await _offerService.GetForCreateAsync()).AllProducts;
            return View(model);
        }

        try
        {
            await _offerService.CreateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.AllProducts = (await _offerService.GetForCreateAsync()).AllProducts;
            return View(model);
        }

        TempData["SuccessMessage"] = "تم إضافة العرض بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var model = await _offerService.GetForEditAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(OfferFormViewModel model)
    {
        BindSpecialPrices(model);

        if (!ModelState.IsValid)
        {
            model.AllProducts = (await _offerService.GetForCreateAsync()).AllProducts;
            return View(model);
        }

        try
        {
            await _offerService.UpdateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.AllProducts = (await _offerService.GetForCreateAsync()).AllProducts;
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تعديل العرض بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _offerService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف العرض بنجاح";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        await _offerService.ToggleActiveAsync(id);
        return RedirectToAction(nameof(Index));
    }

    // Reads the "SpecialPrice_{productId}" form fields posted alongside the checkbox list
    private void BindSpecialPrices(OfferFormViewModel model)
    {
        foreach (var productId in model.SelectedProductIds)
        {
            var key = $"SpecialPrice_{productId}";
            if (Request.Form.TryGetValue(key, out var value) &&
                decimal.TryParse(value, out var price))
            {
                model.SpecialPrices[productId] = price;
            }
        }
    }
}
