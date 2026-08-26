namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public Guid SaleUnitId { get; set; }
    public string SaleUnitName { get; set; } = string.Empty;
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

    public bool IsSoldByWeight => false;

    public string UnitLabel => SaleUnitName;

    public string PriceUnitLabel => $"ج.م / {SaleUnitName}";
}
