using Awlad_Zamzam.MVC.Models.Enums;

namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class CartItemViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImagePath { get; set; }
    public SaleUnit SaleUnit { get; set; } = SaleUnit.Piece;
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public string? Note { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;

    public bool IsSoldByWeight => SaleUnit == SaleUnit.Kilogram;

    public string UnitLabel => SaleUnit == SaleUnit.Kilogram ? "كجم" : "قطعة";
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();

    public decimal Total => Items.Sum(i => i.LineTotal);

    // Number of distinct products in the cart (not a sum of quantities, since mixing
    // kilograms and pieces in one total wouldn't mean anything)
    public int ItemsCount => Items.Count;
}
