namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class AdminProductListViewModel
{
    public List<ProductViewModel> Products { get; set; } = new();

    public string? SearchTerm { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
