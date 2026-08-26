using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

// Everything shown on the admin "الإعدادات" page - mirrors the customer Profile page pattern.
public class AdminSettingsViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }

    public EditAdminProfileViewModel EditProfile { get; set; } = new();
    public ChangeAdminPasswordViewModel ChangePassword { get; set; } = new();
    public StoreSettingsViewModel StoreSettings { get; set; } = new();
}

// Business-wide info shared across all branches, shown on the site footer/contact page - editable by the admin.
// Per-branch info (address, phone, hours, map) is managed separately via BranchFormViewModel.
public class StoreSettingsViewModel
{
    [Required(ErrorMessage = "اسم الصيدلية مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم لا يجب أن يتجاوز 100 حرف")]
    [Display(Name = "اسم الصيدلية")]
    public string PharmacyName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "رقم الواتساب غير صحيح")]
    [Display(Name = "رقم الواتساب")]
    public string? WhatsAppNumber { get; set; }

    [Url(ErrorMessage = "رابط فيسبوك غير صحيح")]
    [Display(Name = "رابط صفحة فيسبوك")]
    public string? FacebookUrl { get; set; }

    [Url(ErrorMessage = "رابط إنستجرام غير صحيح")]
    [Display(Name = "رابط إنستجرام")]
    public string? InstagramUrl { get; set; }
}

public class EditAdminProfileViewModel
{
    [Required(ErrorMessage = "الاسم مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم لا يجب أن يتجاوز 100 حرف")]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }

    [StringLength(250, ErrorMessage = "العنوان لا يجب أن يتجاوز 250 حرف")]
    [Display(Name = "العنوان")]
    public string? Address { get; set; }
}

public class ChangeAdminPasswordViewModel
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الحالية")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 أحرف")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الجديدة")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [Display(Name = "تأكيد كلمة المرور الجديدة")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
