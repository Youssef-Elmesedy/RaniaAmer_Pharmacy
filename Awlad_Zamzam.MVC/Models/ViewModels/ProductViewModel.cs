namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class ProductViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
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
}
