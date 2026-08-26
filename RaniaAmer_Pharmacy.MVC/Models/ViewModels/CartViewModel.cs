namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class CartItemViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImagePath { get; set; }
    public Guid SaleUnitId { get; set; }
    public string SaleUnitName { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal Quantity { get; set; }
    public string? Note { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;

    public bool IsSoldByWeight => false;

    public string UnitLabel => SaleUnitName;
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();

    public decimal Total => Items.Sum(i => i.LineTotal);

    // Number of distinct products in the cart (not a sum of quantities, since mixing
    // different sale units in one total wouldn't mean anything)
    public int ItemsCount => Items.Count;
}
