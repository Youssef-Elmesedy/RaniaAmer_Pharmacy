using System.ComponentModel.DataAnnotations;

namespace RaniaAmer_Pharmacy.MVC.Models.ViewModels;

// Lets the admin log a credit ("آجل") sale directly against a customer,
// without the customer going through the public cart/checkout flow.
public class CreditOrderFormViewModel
{
    // An existing customer, chosen from the search/select list
    public Guid? CustomerId { get; set; }

    // Used only when no existing customer is selected (registers a new one)
    [StringLength(50, ErrorMessage = "الاسم لا يجب أن يتجاوز 50 حرف")]
    [Display(Name = "اسم العميل الجديد")]
    public string? NewCustomerName { get; set; }

    [StringLength(11, MinimumLength = 11, ErrorMessage = "رقم الهاتف يجب أن يكون 11 رقم")]
    [Display(Name = "رقم هاتف العميل الجديد")]
    public string? NewCustomerPhone { get; set; }

    [StringLength(250, ErrorMessage = "العنوان لا يجب أن يتجاوز 250 حرف")]
    [Display(Name = "عنوان العميل الجديد")]
    public string? NewCustomerAddress { get; set; }

    [StringLength(500, ErrorMessage = "الملاحظات لا يجب أن تتجاوز 500 حرف")]
    [Display(Name = "ملاحظات على الطلب")]
    public string? Notes { get; set; }

    public List<Guid> SelectedProductIds { get; set; } = new();

    public Dictionary<Guid, decimal> Quantities { get; set; } = new();

    public List<CustomerSelectItem> Customers { get; set; } = new();

    public List<ProductSelectItem> AllProducts { get; set; } = new();
}
