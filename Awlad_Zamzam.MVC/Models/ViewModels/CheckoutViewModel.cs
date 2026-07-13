using System.ComponentModel.DataAnnotations;

namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class CheckoutViewModel
{
    // Required only for guest checkout; a logged-in customer's Name/Phone come from their account
    [StringLength(50, ErrorMessage = "الاسم لا يجب أن يتجاوز 50 حرف")]
    [Display(Name = "الاسم")]
    public string? Name { get; set; }

    [StringLength(11, MinimumLength = 11, ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم")]
    [Display(Name = "رقم الهاتف")]
    public string? PhoneNumber { get; set; }

    // Optional: a logged-in customer can leave this blank to keep their saved address
    [StringLength(250, ErrorMessage = "العنوان لا يجب أن يتجاوز 250 حرف")]
    [Display(Name = "العنوان")]
    public string? Address { get; set; }

    [StringLength(500, ErrorMessage = "الملاحظات لا يجب أن تتجاوز 500 حرف")]
    [Display(Name = "ملاحظات على الطلب")]
    public string? Notes { get; set; }

    public bool IsLoggedIn { get; set; }

    public string? SavedAddress { get; set; }

    public CartViewModel Cart { get; set; } = new();
}
