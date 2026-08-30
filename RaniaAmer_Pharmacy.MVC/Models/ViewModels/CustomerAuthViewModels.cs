using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

// Fixed list of security questions the customer picks from at registration
public static class SecurityQuestions
{
    public static readonly IReadOnlyList<string> All = new List<string>
    {
        "ما هو اسم مدينتك التي ولدت فيها؟",
        "ما هو اسم والدتك قبل الزواج؟",
        "ما هو اسم أول مدرسة التحقت بها؟",
        "ما هو اسم حيوانك الأليف الأول؟",
        "ما هو لقبك المفضل في الطفولة؟"
    };
}

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
    [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب ألا تقل عن 8 أحرف")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير (A-Z) ورقم واحد على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "سؤال الأمان مطلوب")]
    [Display(Name = "سؤال الأمان")]
    public string SecurityQuestion { get; set; } = string.Empty;

    [Required(ErrorMessage = "إجابة سؤال الأمان مطلوبة")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "الإجابة يجب ألا تقل عن حرفين")]
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

// Step 1 of "forgot password": look up the account's security question by phone number
public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Display(Name = "رقم الهاتف")]
    public string PhoneNumber { get; set; } = string.Empty;
}

// Step 2 of "forgot password": answer the security question and set a new password
public class ResetPasswordViewModel
{
    [Required]
    public string PhoneNumber { get; set; } = string.Empty;

    [Display(Name = "سؤال الأمان")]
    public string SecurityQuestion { get; set; } = string.Empty;

    [Required(ErrorMessage = "إجابة سؤال الأمان مطلوبة")]
    [Display(Name = "إجابة سؤال الأمان")]
    public string SecurityAnswer { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور الجديدة مطلوبة")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "كلمة المرور يجب ألا تقل عن 8 أحرف")]
    [RegularExpression(@"^(?=.*[A-Z])(?=.*\d).+$", ErrorMessage = "كلمة المرور يجب أن تحتوي على حرف كبير (A-Z) ورقم واحد على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور الجديدة")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "كلمتا المرور غير متطابقتين")]
    [Display(Name = "تأكيد كلمة المرور الجديدة")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
