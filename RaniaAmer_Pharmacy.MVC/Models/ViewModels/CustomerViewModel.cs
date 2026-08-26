using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

// Used for the "Contact Us" form: customer leaves name, phone and address to be contacted back
public class CustomerViewModel
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
