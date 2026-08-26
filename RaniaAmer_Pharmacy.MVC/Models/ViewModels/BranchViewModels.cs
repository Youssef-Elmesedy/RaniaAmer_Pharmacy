using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class BranchViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string WorkingHours { get; set; } = string.Empty;
    public string? DeliveryAreaText { get; set; }
    public string? MapEmbedUrl { get; set; }
    public string? MapDirectionsUrl { get; set; }
    public int DisplayOrder { get; set; }
}

public class BranchFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم الفرع مطلوب")]
    [StringLength(100, ErrorMessage = "الاسم لا يجب أن يتجاوز 100 حرف")]
    [Display(Name = "اسم الفرع")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    [Display(Name = "رقم هاتف الفرع")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "العنوان مطلوب")]
    [StringLength(300, ErrorMessage = "العنوان لا يجب أن يتجاوز 300 حرف")]
    [Display(Name = "العنوان")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "مواعيد العمل مطلوبة")]
    [StringLength(200, ErrorMessage = "مواعيد العمل لا يجب أن تتجاوز 200 حرف")]
    [Display(Name = "مواعيد العمل")]
    public string WorkingHours { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "نطاق التوصيل لا يجب أن يتجاوز 200 حرف")]
    [Display(Name = "نطاق التوصيل")]
    public string? DeliveryAreaText { get; set; }

    [Url(ErrorMessage = "رابط تضمين الخريطة غير صحيح")]
    [Display(Name = "رابط تضمين الخريطة (Embed)")]
    public string? MapEmbedUrl { get; set; }

    [Url(ErrorMessage = "رابط الاتجاهات غير صحيح")]
    [Display(Name = "رابط الاتجاهات")]
    public string? MapDirectionsUrl { get; set; }

    [Display(Name = "ترتيب الظهور")]
    public int DisplayOrder { get; set; }
}
