using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RaniaAmer_Pharmacy.MVC.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class DashboardController : Controller
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IOfferRepository _offerRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IOrderService _orderService;

    public DashboardController(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        ICustomerRepository customerRepository,
        IOfferRepository offerRepository,
        IOrderRepository orderRepository,
        IOrderService orderService)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _customerRepository = customerRepository;
        _offerRepository = offerRepository;
        _orderRepository = orderRepository;
        _orderService = orderService;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _productRepository.GetAllAsync();
        var (totalOutstanding, totalPaid) = await _orderRepository.GetCreditTotalsAsync();

        var model = new DashboardViewModel
        {
            ProductsCount = products.Count,
            AvailableProductsCount = products.Count(p => p.IsAvailable),
            OffersCount = products.Count(p => p.DiscountPercentage > 0),
            CategoriesCount = await _categoryRepository.CountAsync(),
            CustomersCount = await _customerRepository.CountAsync(),
            BundleOffersCount = await _offerRepository.CountAsync(),
            PendingOrdersCount = await _orderService.GetPendingCountAsync(),
            TotalCreditOutstanding = totalOutstanding,
            TotalCreditPaid = totalPaid
        };

        return View(model);
    }
}
