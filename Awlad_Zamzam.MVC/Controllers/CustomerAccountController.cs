using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Awlad_Zamzam.MVC.Controllers;

// Customer-facing login/registration, separate from the Admin identity system
public class CustomerAccountController : Controller
{
    public const string SchemeName = "CustomerScheme";

    private readonly ICustomerAuthService _customerAuthService;
    private readonly IOrderService _orderService;

    public CustomerAccountController(ICustomerAuthService customerAuthService, IOrderService orderService)
    {
        _customerAuthService = customerAuthService;
        _orderService = orderService;
    }

    [HttpGet]
    public IActionResult Register() => View(new CustomerRegisterViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(CustomerRegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var customer = await _customerAuthService.RegisterAsync(model);
            await SignInCustomerAsync(customer.Id, customer.Name);

            TempData["SuccessMessage"] = "تم إنشاء الحساب بنجاح";
            return RedirectToAction(nameof(MyOrders));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult Login() => View(new CustomerLoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CustomerLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var customer = await _customerAuthService.ValidateLoginAsync(model);

        if (customer == null)
        {
            ModelState.AddModelError(string.Empty, "رقم الهاتف أو كلمة المرور غير صحيحة");
            return View(model);
        }

        await SignInCustomerAsync(customer.Id, customer.Name);

        return RedirectToAction(nameof(MyOrders));
    }

    [HttpGet]
    public IActionResult LoggedOut()
    {
        if (TempData["LoggedOut"] == null)
            return RedirectToAction("Index", "Home");

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(SchemeName);

        TempData["LoggedOut"] = true;

        Response.Cookies.Delete("CustomerAuth");
        Response.Cookies.Delete(".AspNetCore.CustomerScheme");

        return RedirectToAction(nameof(LoggedOut));
    }

    [Authorize(AuthenticationSchemes = SchemeName)]
    public async Task<IActionResult> MyOrders()
    {
        var customerId = GetCustomerId();
        var orders = await _orderService.GetByCustomerAsync(customerId);
        return View(orders);
    }

    [Authorize(AuthenticationSchemes = SchemeName)]
    public async Task<IActionResult> MyCreditOrders()
    {
        var customerId = GetCustomerId();
        var orders = await _orderService.GetCreditOrdersByCustomerAsync(customerId);
        return View(orders);
    }

    [Authorize(AuthenticationSchemes = SchemeName)]
    public async Task<IActionResult> OrderDetails(Guid id)
    {
        var customerId = GetCustomerId();
        var orders = await _orderService.GetByCustomerAsync(customerId);

        // Make sure the order actually belongs to the logged-in customer
        if (!orders.Any(o => o.Id == id))
            return Forbid();

        var details = await _orderService.GetDetailsAsync(id);
        if (details == null) return NotFound();

        return View(details);
    }

    private Guid GetCustomerId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private async Task SignInCustomerAsync(Guid customerId, string name)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, customerId.ToString()),
            new(ClaimTypes.Name, name)
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(SchemeName, principal, new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(30)
        });
    }
}
