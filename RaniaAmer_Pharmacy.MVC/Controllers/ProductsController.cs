using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RaniaAmer_Pharmacy.MVC.Controllers;

// Public-facing menu: Fresh Meat / Processed Meat / Grills, with search, sort and paging
public class ProductsController : Controller
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    public async Task<IActionResult> Index(Guid? categoryId, string? search, string sortOrder = "default", int page = 1)
    {
        var model = await _productService.GetListAsync(categoryId, search, sortOrder, page, pageSize: 6);
        return View(model);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var product = await _productService.GetDetailsAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }
}
