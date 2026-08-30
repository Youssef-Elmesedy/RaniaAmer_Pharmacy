using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RaniaAmer_Pharmacy.MVC.Components.CartSummary;

// Renders the cart icon + items badge in the main navbar. Rendered on every public page.
public class CartSummaryViewComponent : ViewComponent
{
    private readonly ICartService _cartService;
    private readonly ILogger<CartSummaryViewComponent> _logger;

    public CartSummaryViewComponent(ICartService cartService, ILogger<CartSummaryViewComponent> logger)
    {
        _cartService = cartService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var itemsCount = 0;

        try
        {
            itemsCount = await _cartService.GetItemsCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load the cart summary.");
        }

        return View(itemsCount);
    }
}
