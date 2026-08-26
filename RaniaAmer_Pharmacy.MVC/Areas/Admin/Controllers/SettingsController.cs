using RaniaAmer_Pharmacy.MVC.Models.Entities;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace RaniaAmer_Pharmacy.MVC.Areas.Admin.Controllers;

// Admin's own account settings: profile info + change password.
// Uses ASP.NET Core Identity's UserManager directly (not a custom service) since Identity
// already handles password hashing/validation/lockout policy correctly - no need to reinvent it.
[Authorize(Roles = "Admin")]
[Area("Admin")]
public class SettingsController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ISiteSettingsService _siteSettingsService;
    private readonly ILogger<SettingsController> _logger;

    public SettingsController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ISiteSettingsService siteSettingsService,
        ILogger<SettingsController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _siteSettingsService = siteSettingsService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var model = BuildViewModel(user);
        model.StoreSettings = await _siteSettingsService.GetAsync();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile([Bind(Prefix = "EditProfile")] EditAdminProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return await ReturnSettingsWithErrors(profileModel: model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.FullName = model.FullName.Trim();
        user.Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim();

        if (!string.IsNullOrWhiteSpace(model.PhoneNumber) && model.PhoneNumber.Trim() != user.PhoneNumber)
        {
            var phoneResult = await _userManager.SetPhoneNumberAsync(user, model.PhoneNumber.Trim());
            if (!phoneResult.Succeeded)
            {
                foreach (var error in phoneResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return await ReturnSettingsWithErrors(profileModel: model);
            }
        }

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return await ReturnSettingsWithErrors(profileModel: model);
        }

        TempData["SuccessMessage"] = "تم تحديث بيانات الملف الشخصي بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword([Bind(Prefix = "ChangePassword")] ChangeAdminPasswordViewModel model)
    {
        if (!ModelState.IsValid)
            return await ReturnSettingsWithErrors(passwordModel: model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, TranslateIdentityError(error));

            return await ReturnSettingsWithErrors(passwordModel: model);
        }

        // Refresh the sign-in cookie so the current session stays valid after the password change
        await _signInManager.RefreshSignInAsync(user);

        TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditStoreSettings([Bind(Prefix = "StoreSettings")] StoreSettingsViewModel model)
    {
        if (!ModelState.IsValid)
            return await ReturnSettingsWithErrors(storeSettingsModel: model);

        try
        {
            await _siteSettingsService.UpdateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return await ReturnSettingsWithErrors(storeSettingsModel: model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while updating store settings.");
            ModelState.AddModelError(string.Empty, "حدث خطأ غير متوقع أثناء الحفظ. حاول مرة أخرى.");
            return await ReturnSettingsWithErrors(storeSettingsModel: model);
        }

        TempData["SuccessMessage"] = "تم تحديث بيانات المتجر بنجاح";
        return RedirectToAction(nameof(Index));
    }

    private async Task<IActionResult> ReturnSettingsWithErrors(
        EditAdminProfileViewModel? profileModel = null,
        ChangeAdminPasswordViewModel? passwordModel = null,
        StoreSettingsViewModel? storeSettingsModel = null)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var vm = BuildViewModel(user);
        vm.StoreSettings = storeSettingsModel ?? await _siteSettingsService.GetAsync();

        if (profileModel != null)
        {
            vm.EditProfile = profileModel;
            ViewData["ActiveSettingsTab"] = "profile";
        }
        if (passwordModel != null)
        {
            vm.ChangePassword = passwordModel;
            ViewData["ActiveSettingsTab"] = "password";
        }
        if (storeSettingsModel != null)
        {
            ViewData["ActiveSettingsTab"] = "store";
        }

        return View(nameof(Index), vm);
    }

    private static AdminSettingsViewModel BuildViewModel(ApplicationUser user) => new()
    {
        FullName = user.FullName,
        Email = user.Email ?? string.Empty,
        PhoneNumber = user.PhoneNumber,
        Address = user.Address,
        EditProfile = new EditAdminProfileViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address
        }
    };

    // ASP.NET Core Identity's built-in error messages are in English by default
    private static string TranslateIdentityError(IdentityError error) => error.Code switch
    {
        "PasswordMismatch" => "كلمة المرور الحالية غير صحيحة",
        "PasswordTooShort" => "كلمة المرور قصيرة جدًا",
        "PasswordRequiresNonAlphanumeric" => "كلمة المرور يجب أن تحتوي على رمز غير أبجدي رقمي",
        "PasswordRequiresDigit" => "كلمة المرور يجب أن تحتوي على رقم واحد على الأقل",
        "PasswordRequiresUpper" => "كلمة المرور يجب أن تحتوي على حرف كبير واحد على الأقل",
        "PasswordRequiresLower" => "كلمة المرور يجب أن تحتوي على حرف صغير واحد على الأقل",
        _ => error.Description
    };
}
