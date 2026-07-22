using Awlad_Zamzam.MVC.Models.Enums;

namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public SaleUnit SaleUnit { get; set; } = SaleUnit.Piece;
    public int DiscountPercentage { get; set; }
    public string? ImagePath { get; set; }
    public bool IsAvailable { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public decimal PriceAfterDiscount =>
        DiscountPercentage > 0
            ? Math.Round(Price - (Price * DiscountPercentage / 100), 2)
            : Price;

    public bool HasDiscount => DiscountPercentage > 0;

    public bool IsSoldByWeight => SaleUnit == SaleUnit.Kilogram;

    public string UnitLabel => SaleUnit == SaleUnit.Kilogram ? "كجم" : "قطعة";

    public string PriceUnitLabel => SaleUnit == SaleUnit.Kilogram ? "ج.م / كجم" : "ج.م / قطعة";
}
