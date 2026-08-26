namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class ProductListViewModel
{
    public List<ProductViewModel> Products { get; set; } = new();
    public List<CategorySelectItem> Categories { get; set; } = new();

    public Guid? CurrentCategoryId { get; set; }
    public string? SearchTerm { get; set; }
    public string SortOrder { get; set; } = "default";

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 6;
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}
