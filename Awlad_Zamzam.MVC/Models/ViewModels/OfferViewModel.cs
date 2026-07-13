namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class OfferViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<OfferItemViewModel> Items { get; set; } = new();

    public decimal TotalOriginalPrice => Items.Sum(i => i.OriginalPrice);
    public decimal TotalSpecialPrice => Items.Sum(i => i.SpecialPrice);
    public decimal TotalSavings => TotalOriginalPrice - TotalSpecialPrice;
}

public class OfferItemViewModel
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImagePath { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal SpecialPrice { get; set; }
}
