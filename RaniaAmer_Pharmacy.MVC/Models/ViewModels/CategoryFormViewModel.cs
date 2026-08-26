using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class CategoryFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم القسم مطلوب")]
    [StringLength(100, ErrorMessage = "اسم القسم لا يجب أن يتجاوز 100 حرف")]
    [Display(Name = "اسم القسم")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "صورة القسم")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImage { get; set; }
}
