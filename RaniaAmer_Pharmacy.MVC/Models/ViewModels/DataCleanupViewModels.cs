namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

// Everything shown on the "تنظيف البيانات" admin page: growing tables needing row cleanup,
// plus customers eligible to be auto-paused for inactivity.
public class DataCleanupPageViewModel
{
    public List<DataCleanupOverviewItem> TableCleanup { get; set; } = new();

    public int InactiveCustomersCount { get; set; }
    public List<CustomerListItemViewModel> InactiveCustomersPreview { get; set; } = new();
    public int InactivityThresholdMonths { get; set; }
}

// One "watched table" that has crossed its threshold and is waiting for admin approval to clean up.
public class DataCleanupOverviewItem
{
    public string Key { get; set; } = string.Empty;          // e.g. "Orders" — used to route the Delete action
    public string DisplayName { get; set; } = string.Empty;  // e.g. "الطلبات"

    public int TotalCount { get; set; }
    public int Threshold { get; set; }

    // How many of the oldest rows are actually safe to delete right now (e.g. excludes unpaid
    // credit orders). Capped at the table's MaxDeleteCap (50,000 for Orders).
    public int EligibleCount { get; set; }

    public int MaxDeleteCap { get; set; }
}
