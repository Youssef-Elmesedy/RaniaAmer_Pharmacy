namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class CartItemViewModel
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImagePath { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }

    public decimal LineTotal => UnitPrice * Quantity;
}

public class CartViewModel
{
    public List<CartItemViewModel> Items { get; set; } = new();

    public decimal Total => Items.Sum(i => i.LineTotal);

    public int ItemsCount => Items.Sum(i => i.Quantity);
}
