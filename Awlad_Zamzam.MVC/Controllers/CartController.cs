using System.Security.Claims;
using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;

    public CartController(ICartService cartService, IOrderService orderService, ICustomerService customerService)
    {
        _cartService = cartService;
        _orderService = orderService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index()
    {
        var cart = await _cartService.GetCartAsync();
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(Guid productId, decimal quantity = 1, string? note = null, string? returnUrl = null)
    {
        try
        {
            await _cartService.AddItemAsync(productId, quantity, note);
            TempData["SuccessMessage"] = "تم إضافة المنتج إلى السلة";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(Guid productId, decimal quantity)
    {
        await _cartService.UpdateQuantityAsync(productId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(Guid productId)
    {
        await _cartService.RemoveItemAsync(productId);
        TempData["SuccessMessage"] = "تم حذف المنتج من السلة";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Checkout()
    {
        var cart = await _cartService.GetCartAsync();

        if (!cart.Items.Any())
        {
            TempData["ErrorMessage"] = "السلة فارغة";
            return RedirectToAction(nameof(Index));
        }

        var model = new CheckoutViewModel { Cart = cart };

        var customerId = await GetAuthenticatedCustomerIdAsync();
        if (customerId.HasValue)
        {
            var customer = await _customerService.GetByIdAsync(customerId.Value);
            if (customer != null)
            {
                model.IsLoggedIn = true;
                model.Name = customer.Name;
                model.PhoneNumber = customer.PhoneNumber;
                model.SavedAddress = customer.Address;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutViewModel model)
    {
        model.Cart = await _cartService.GetCartAsync();

        if (!model.Cart.Items.Any())
        {
            TempData["ErrorMessage"] = "السلة فارغة";
            return RedirectToAction(nameof(Index));
        }

        var customerId = await GetAuthenticatedCustomerIdAsync();
        model.IsLoggedIn = customerId.HasValue;

        if (!model.IsLoggedIn)
        {
            // Guest checkout: name/phone/address are required
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "الاسم مطلوب");
            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                ModelState.AddModelError(nameof(model.PhoneNumber), "رقم الهاتف مطلوب");
            if (string.IsNullOrWhiteSpace(model.Address))
                ModelState.AddModelError(nameof(model.Address), "العنوان مطلوب");
        }
        else
        {
            var customer = await _customerService.GetByIdAsync(customerId!.Value);
            if (customer != null) model.SavedAddress = customer.Address;
        }

        if (!ModelState.IsValid)
            return View(model);

        Guid orderId;
        try
        {
            orderId = await _orderService.CreateFromCartAsync(model, customerId);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        return RedirectToAction(nameof(Confirmation), new { id = orderId });
    }

    public IActionResult Confirmation(Guid id)
    {
        ViewBag.OrderId = id;
        return View();
    }

    private async Task<Guid?> GetAuthenticatedCustomerIdAsync()
    {
        var result = await HttpContext.AuthenticateAsync(CustomerAccountController.SchemeName);
        if (!result.Succeeded) return null;

        var idClaim = result.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(idClaim, out var id) ? id : null;
    }
}
