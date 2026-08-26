using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

// Read-only summary shown at the top of the profile page
public class CustomerProfileViewModel
{
    public string Name { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? SecurityQuestion { get; set; }

    public EditProfileViewModel EditProfile { get; set; } = new();

    public ChangePasswordViewModel ChangePassword { get; set; } = new();

    public ChangeSecurityQuestionViewModel ChangeSecurityQuestion { get; set; } = new();
}

public class EditProfileViewModel
{
    [Required(ErrorMessage = "الاسم مطلوب")]
    [StringLength(50, ErrorMessage = "الاسم لا يجب أن يتجاوز 50 حرف")]
    [Display(Name = "الاسم")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [StringLength(11, MinimumLength = 11, ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم")]
    [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(250, ErrorMessage = "العنوان لا يجب أن يتجاوز 250 حرف")]
    [Display(Name = "العنوان")]
    public string Address { get; set; } = string.Empty;
}

public class ChangePasswordViewModel
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

public class ChangeSecurityQuestionViewModel
{
    [Required(ErrorMessage = "كلمة المرور الحالية مطلوبة")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الحالية")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "سؤال الأمان مطلوب")]
    [Display(Name = "سؤال الأمان")]
    public string SecurityQuestion { get; set; } = string.Empty;

    [Required(ErrorMessage = "إجابة سؤال الأمان مطلوبة")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "الإجابة يجب ألا تقل عن حرفين")]
    [Display(Name = "إجابة سؤال الأمان")]
    public string SecurityAnswer { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد الإجابة مطلوب")]
    [Compare(nameof(SecurityAnswer), ErrorMessage = "الإجابتان غير متطابقتين")]
    [Display(Name = "تأكيد الإجابة")]
    public string ConfirmSecurityAnswer { get; set; } = string.Empty;
}
