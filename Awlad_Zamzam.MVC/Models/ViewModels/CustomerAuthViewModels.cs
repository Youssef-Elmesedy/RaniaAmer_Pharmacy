using System.ComponentModel.DataAnnotations;

namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class CustomerRegisterViewModel
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

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 أحرف")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    // سؤال الأمان
    [Required(ErrorMessage = "اختر سؤال الأمان")]
    [Display(Name = "سؤال الأمان")]
    public string SecurityQuestion { get; set; } = string.Empty;

    // إجابة سؤال الأمان
    [Required(ErrorMessage = "أدخل إجابة سؤال الأمان")]
    [StringLength(100, ErrorMessage = "الإجابة طويلة جداً")]
    [Display(Name = "إجابة سؤال الأمان")]
    public string SecurityAnswer { get; set; } = string.Empty;
}

public class CustomerLoginViewModel
{
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;
}
