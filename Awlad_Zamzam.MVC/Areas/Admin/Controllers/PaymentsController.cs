using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Areas.Admin.Controllers;

// A dedicated page to record credit ("آجل") payments without navigating through the customer's own page.
// The admin picks a customer directly here (dropdown / search by name or phone).
[Authorize(Roles = "Admin")]
[Area("Admin")]
public class PaymentsController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;

    public PaymentsController(ICustomerService customerService, IOrderService orderService)
    {
        _customerService = customerService;
        _orderService = orderService;
    }

    public async Task<IActionResult> Index(Guid? customerId, string? search)
    {
        var customers = await _customerService.SearchAsync(search);

        var model = new PaymentPageViewModel
        {
            SearchTerm = search,
            Customers = customers
                .Select(c => new CustomerSelectItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    PhoneNumber = c.PhoneNumber,
                    TotalCreditDue = c.TotalCreditDue
                })
                .ToList()
        };

        if (customerId.HasValue)
        {
            var customer = await _customerService.GetByIdAsync(customerId.Value);
            if (customer != null)
            {
                model.SelectedCustomerId = customer.Id;
                model.SelectedCustomerName = customer.Name;
                model.SelectedCustomerPhone = customer.PhoneNumber;

                var creditOrders = await _orderService.GetCreditOrdersByCustomerAsync(customerId.Value);
                model.CreditOrders = creditOrders.OrderByDescending(o => o.OrderDate).ToList();

                model.PaymentsLog = await _orderService.GetPaymentsLogByCustomerAsync(customerId.Value);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(Guid customerId, decimal amount, string? notes)
    {
        try
        {
            await _orderService.PayCustomerCreditAsync(customerId, amount, notes);
            TempData["SuccessMessage"] = "تم تسجيل الدفعة وتوزيعها على الطلبات بنجاح";
        }
        catch (BusinessException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index), new { customerId });
    }
}
