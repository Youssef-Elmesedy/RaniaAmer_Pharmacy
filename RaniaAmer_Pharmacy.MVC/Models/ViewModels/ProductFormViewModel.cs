using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class ProductFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم المنتج مطلوب")]
    [StringLength(50, ErrorMessage = "اسم المنتج لا يجب أن يتجاوز 50 حرف")]
    [Display(Name = "اسم المنتج")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "الوصف مطلوب")]
    [StringLength(100, ErrorMessage = "الوصف لا يجب أن يتجاوز 100 حرف")]
    [Display(Name = "الوصف")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "السعر مطلوب")]
    [Range(0.01, 100000, ErrorMessage = "السعر يجب أن يكون أكبر من صفر")]
    [Display(Name = "السعر")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "وحدة البيع مطلوبة")]
    [Display(Name = "يُباع بـ")]
    public Guid SaleUnitId { get; set; }

    [Range(0, 100, ErrorMessage = "نسبة الخصم يجب أن تكون بين 0 و 100")]
    [Display(Name = "نسبة الخصم %")]
    public int DiscountPercentage { get; set; }

    [Display(Name = "صورة المنتج")]
    public IFormFile? ImageFile { get; set; }

    public string? ExistingImagePath { get; set; }

    [Required(ErrorMessage = "القسم مطلوب")]
    [Display(Name = "القسم")]
    public Guid CategoryId { get; set; }

    [Display(Name = "متوفر")]
    public bool IsAvailable { get; set; } = true;

    public List<CategorySelectItem> Categories { get; set; } = new();

    public List<SaleUnitSelectItem> SaleUnits { get; set; } = new();

    // Posted as two parallel arrays from the dynamic "sub-units" rows in the form
    // (index i in one list pairs with index i in the other)
    public List<Guid>? UnitOptionSaleUnitIds { get; set; }
    public List<int>? UnitOptionQuantities { get; set; }

    // Existing sub-unit options, used to pre-fill the form rows when editing a product
    public List<ProductUnitOptionInput> ExistingUnitOptions { get; set; } = new();
}

public class ProductUnitOptionInput
{
    public Guid SaleUnitId { get; set; }
    public int QuantityPerBaseUnit { get; set; }
}

public class CategorySelectItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
