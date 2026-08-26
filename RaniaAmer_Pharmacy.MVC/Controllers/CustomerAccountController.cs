using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RaniaAmer_Pharmacy.MVC.Controllers;

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
    public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var question = await _customerAuthService.GetSecurityQuestionAsync(model.PhoneNumber);

        if (question == null)
        {
            ModelState.AddModelError(string.Empty, "لا يوجد حساب مرتبط بهذا الرقم أو لم يتم تعيين سؤال أمان له");
            return View(model);
        }

        return RedirectToAction(nameof(ResetPassword), new { phoneNumber = model.PhoneNumber });
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string phoneNumber)
    {
        var question = await _customerAuthService.GetSecurityQuestionAsync(phoneNumber);

        if (question == null)
            return RedirectToAction(nameof(ForgotPassword));

        return View(new ResetPasswordViewModel
        {
            PhoneNumber = phoneNumber,
            SecurityQuestion = question
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        // Re-fetch the question in case someone tampered with the hidden/readonly field
        var question = await _customerAuthService.GetSecurityQuestionAsync(model.PhoneNumber);
        model.SecurityQuestion = question ?? model.SecurityQuestion;

        if (!ModelState.IsValid)
            return View(model);

        var success = await _customerAuthService.ResetPasswordAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, "إجابة سؤال الأمان غير صحيحة");
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح، يمكنك تسجيل الدخول الآن";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View(new CustomerLoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(CustomerLoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        Customer? customer;
        try
        {
            customer = await _customerAuthService.ValidateLoginAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        if (customer == null)
        {
            ModelState.AddModelError(string.Empty, "رقم الهاتف أو كلمة المرور غير صحيحة");
            return View(model);
        }

        await SignInCustomerAsync(customer.Id, customer.Name);

        return RedirectToAction(nameof(MyOrders));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult LoggedOut()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(SchemeName);

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

    [Authorize(AuthenticationSchemes = SchemeName)]
    public async Task<IActionResult> Profile()
    {
        var customer = await _customerAuthService.GetProfileAsync(GetCustomerId());
        if (customer == null) return NotFound();

        return View(BuildProfileViewModel(customer));
    }

    [Authorize(AuthenticationSchemes = SchemeName)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile([Bind(Prefix = "EditProfile")] EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return await ReturnProfileWithErrors(model: model);

        try
        {
            var customerId = GetCustomerId();
            await _customerAuthService.UpdateProfileAsync(customerId, model);

            TempData["SuccessMessage"] = "تم تحديث بيانات الملف الشخصي بنجاح";
            return RedirectToAction(nameof(Profile));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReturnProfileWithErrors(model: model);
        }
    }

    [Authorize(AuthenticationSchemes = SchemeName)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return await ReturnProfileWithErrors(passwordModel: model);

        try
        {
            await _customerAuthService.ChangePasswordAsync(GetCustomerId(), model);

            TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح";
            return RedirectToAction(nameof(Profile));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReturnProfileWithErrors(passwordModel: model);
        }
    }

    [Authorize(AuthenticationSchemes = SchemeName)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeSecurityQuestion([Bind(Prefix = "ChangeSecurityQuestion")] ChangeSecurityQuestionViewModel model)
    {
        if (!ModelState.IsValid)
            return await ReturnProfileWithErrors(securityModel: model);

        try
        {
            await _customerAuthService.ChangeSecurityQuestionAsync(GetCustomerId(), model);

            TempData["SuccessMessage"] = "تم تحديث سؤال الأمان بنجاح";
            return RedirectToAction(nameof(Profile));
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReturnProfileWithErrors(securityModel: model);
        }
    }

    // Re-renders the Profile view after a failed sub-form submission, keeping the entered
    // values for that sub-form while reloading fresh data for the others.
    private async Task<IActionResult> ReturnProfileWithErrors(
        EditProfileViewModel? model = null,
        ChangePasswordViewModel? passwordModel = null,
        ChangeSecurityQuestionViewModel? securityModel = null)
    {
        var customer = await _customerAuthService.GetProfileAsync(GetCustomerId());
        if (customer == null) return NotFound();

        var vm = BuildProfileViewModel(customer);

        if (model != null)
        {
            vm.EditProfile = model;
            ViewData["ActiveProfileTab"] = "info";
        }
        if (passwordModel != null)
        {
            vm.ChangePassword = passwordModel;
            ViewData["ActiveProfileTab"] = "password";
        }
        if (securityModel != null)
        {
            vm.ChangeSecurityQuestion = securityModel;
            ViewData["ActiveProfileTab"] = "security";
        }

        return View(nameof(Profile), vm);
    }

    private static CustomerProfileViewModel BuildProfileViewModel(Customer customer) => new()
    {
        Name = customer.Name,
        PhoneNumber = customer.PhoneNumber,
        Address = customer.Address,
        SecurityQuestion = customer.SecurityQuestion,
        EditProfile = new EditProfileViewModel
        {
            Name = customer.Name,
            PhoneNumber = customer.PhoneNumber,
            Address = customer.Address
        }
    };

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
