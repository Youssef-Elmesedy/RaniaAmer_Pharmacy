using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Controllers;

public class OffersController : Controller
{
    private readonly IOfferService _offerService;

    public OffersController(IOfferService offerService)
    {
        _offerService = offerService;
    }

    public async Task<IActionResult> Index()
    {
        var offers = await _offerService.GetActiveOffersAsync();
        return View(offers);
    }

    // Polled from the public pages so customers see new offers without a manual refresh
    [HttpGet]
    public async Task<IActionResult> Count()
    {
        var offers = await _offerService.GetActiveOffersAsync();
        return Json(new { count = offers.Count });
    }
}
