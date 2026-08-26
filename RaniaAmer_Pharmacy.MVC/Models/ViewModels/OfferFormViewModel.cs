using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class OfferFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "عنوان العرض مطلوب")]
    [StringLength(100, ErrorMessage = "العنوان لا يجب أن يتجاوز 100 حرف")]
    [Display(Name = "عنوان العرض")]
    public string Title { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "الوصف لا يجب أن يتجاوز 300 حرف")]
    [Display(Name = "الوصف")]
    public string? Description { get; set; }

    [Display(Name = "مفعّل")]
    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "اختر منتج واحد على الأقل")]
    [Display(Name = "المنتجات")]
    public List<Guid> SelectedProductIds { get; set; } = new();

    public Dictionary<Guid, decimal> SpecialPrices { get; set; } = new();

    public List<ProductSelectItem> AllProducts { get; set; } = new();
}

public class ProductSelectItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string SaleUnitName { get; set; } = string.Empty;
    public string? ImagePath { get; set; }

    public string UnitLabel => SaleUnitName;
}
