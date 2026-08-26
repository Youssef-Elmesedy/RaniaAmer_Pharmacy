using System.Diagnostics;
using RaniaAmer_Pharmacy.MVC.Models;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RaniaAmer_Pharmacy.MVC.Controllers;

public class HomeController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IOfferService _offerService;
    private readonly ICatalogChangeTracker _catalogChangeTracker;

    public HomeController(
        IProductService productService,
        ICategoryService categoryService,
        IOfferService offerService,
        ICatalogChangeTracker catalogChangeTracker)
    {
        _productService = productService;
        _categoryService = categoryService;
        _offerService = offerService;
        _catalogChangeTracker = catalogChangeTracker;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Offers = await _productService.GetOffersAsync(6);
        ViewBag.Categories = await _categoryService.GetSelectListAsync();
        ViewBag.BundleOffers = (await _offerService.GetActiveOffersAsync()).Take(4).ToList();
        return View();
    }

    public IActionResult Privacy() => View();

    // Polled from every public page so customers see new/updated products, categories,
    // and offers without needing to manually refresh
    [HttpGet]
    public IActionResult CatalogVersion() => Json(new { version = _catalogChangeTracker.GetVersion() });

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

    public IActionResult Error404() => View();
}
