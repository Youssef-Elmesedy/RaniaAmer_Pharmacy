using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class SaleUnitFormViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "اسم وحدة البيع مطلوب")]
    [StringLength(30, ErrorMessage = "اسم وحدة البيع لا يجب أن يتجاوز 30 حرف")]
    [Display(Name = "اسم وحدة البيع")]
    public string Name { get; set; } = string.Empty;
}

public class SaleUnitSelectItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
