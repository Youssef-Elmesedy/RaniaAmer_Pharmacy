using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class OrdersController : Controller
{
    private readonly IOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IProductRepository _productRepository;

    public OrdersController(
        IOrderService orderService,
        ICustomerService customerService,
        IProductRepository productRepository)
    {
        _orderService = orderService;
        _customerService = customerService;
        _productRepository = productRepository;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _orderService.GetAllAsync();
        return View(orders.OrderByDescending(o => o.OrderDate).ToList());
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var order = await _orderService.GetDetailsAsync(id);
        if (order == null) return NotFound();
        return View(order);
    }

    // Admin logs a credit ("آجل") sale directly against a customer
    public async Task<IActionResult> Create(string? search)
    {
        var model = new CreditOrderFormViewModel
        {
            Customers = await GetCustomerSelectItemsAsync(search),
            AllProducts = await GetProductSelectItemsAsync()
        };

        ViewBag.SearchTerm = search;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreditOrderFormViewModel model)
    {
        BindQuantities(model);

        if (!ModelState.IsValid)
        {
            model.Customers = await GetCustomerSelectItemsAsync(null);
            model.AllProducts = await GetProductSelectItemsAsync();
            return View(model);
        }

        Guid orderId;
        try
        {
            orderId = await _orderService.CreateCreditOrderByAdminAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Customers = await GetCustomerSelectItemsAsync(null);
            model.AllProducts = await GetProductSelectItemsAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تسجيل طلب الآجل بنجاح";
        return RedirectToAction(nameof(Details), new { id = orderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid id, bool isCredit)
    {
        try
        {
            await _orderService.CompleteAsync(id, isCredit);
            TempData["SuccessMessage"] = isCredit
                ? "تم تسليم الطلب بنظام الآجل"
                : "تم تسليم الطلب كاش";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(Guid id, decimal amount, string? notes)
    {
        try
        {
            await _orderService.AddPaymentAsync(id, amount, notes);
            TempData["SuccessMessage"] = "تم تسجيل الدفعة بنجاح";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _orderService.DeleteAsync(id);
            TempData["SuccessMessage"] = "تم حذف الطلب";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<List<CustomerSelectItem>> GetCustomerSelectItemsAsync(string? search)
    {
        var customers = await _customerService.SearchAsync(search);
        return customers
            .Select(c => new CustomerSelectItem
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                TotalCreditDue = c.TotalCreditDue
            })
            .ToList();
    }

    private async Task<List<ProductSelectItem>> GetProductSelectItemsAsync()
    {
        var products = await _productRepository.GetAllAsync();
        return products
            .Where(p => p.IsAvailable)
            .OrderBy(p => p.Name)
            .Select(p => new ProductSelectItem
            {
                Id = p.Id,
                Name = p.Name,
                Price = p.Price,
                ImagePath = p.ImagePath
            })
            .ToList();
    }

    // Reads the "Quantity_{productId}" form fields posted alongside the product checkboxes
    private void BindQuantities(CreditOrderFormViewModel model)
    {
        foreach (var productId in model.SelectedProductIds)
        {
            var key = $"Quantity_{productId}";
            if (Request.Form.TryGetValue(key, out var value) && int.TryParse(value, out var qty))
            {
                model.Quantities[productId] = qty;
            }
        }
    }
}
