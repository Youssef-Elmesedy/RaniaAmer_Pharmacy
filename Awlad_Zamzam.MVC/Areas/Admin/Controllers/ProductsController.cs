using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class ProductsController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ICategoryService categoryService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _categoryService = categoryService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productService.GetAllForAdminAsync();
        return View(products.OrderByDescending(p => p.CreatedAt).ToList());
    }

    public async Task<IActionResult> Create()
    {
        var model = new ProductFormViewModel { Categories = await _categoryService.GetSelectListAsync() };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetSelectListAsync();
            return View(model);
        }

        try
        {
            await _productService.CreateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Categories = await _categoryService.GetSelectListAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            // Safety net: never let an unexpected error (DB connection issue, timeout, etc.)
            // escape as an unhandled exception - show a friendly message instead.
            _logger.LogError(ex, "Unexpected error while creating a product.");
            ModelState.AddModelError(string.Empty,
                "حدث خطأ غير متوقع أثناء الحفظ. تأكد من اتصال قاعدة البيانات وحاول مرة أخرى.");
            model.Categories = await _categoryService.GetSelectListAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "تم إضافة المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var product = await _productService.GetDetailsAsync(id);
        if (product == null) return NotFound();

        var model = new ProductFormViewModel
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            DiscountPercentage = product.DiscountPercentage,
            ExistingImagePath = product.ImagePath,
            CategoryId = product.CategoryId,
            IsAvailable = product.IsAvailable,
            Categories = await _categoryService.GetSelectListAsync()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProductFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetSelectListAsync();
            return View(model);
        }

        try
        {
            await _productService.UpdateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Categories = await _categoryService.GetSelectListAsync();
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating a product.");
            ModelState.AddModelError(string.Empty,
                "حدث خطأ غير متوقع أثناء الحفظ. تأكد من اتصال قاعدة البيانات وحاول مرة أخرى.");
            model.Categories = await _categoryService.GetSelectListAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تعديل المنتج بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _productService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف المنتج بنجاح";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while deleting a product.");
            TempData["ErrorMessage"] = "حدث خطأ غير متوقع أثناء الحذف.";
        }

        return RedirectToAction(nameof(Index));
    }
}
