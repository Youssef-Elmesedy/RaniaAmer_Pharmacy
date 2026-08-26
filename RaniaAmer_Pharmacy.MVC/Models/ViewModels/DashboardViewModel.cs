namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

public class DashboardViewModel
{
    public int ProductsCount { get; set; }
    public int CategoriesCount { get; set; }
    public int AvailableProductsCount { get; set; }
    public int OffersCount { get; set; }
    public int CustomersCount { get; set; }
    public int BundleOffersCount { get; set; }
    public int PendingOrdersCount { get; set; }
    public decimal TotalCreditOutstanding { get; set; }
    public decimal TotalCreditPaid { get; set; }
}
