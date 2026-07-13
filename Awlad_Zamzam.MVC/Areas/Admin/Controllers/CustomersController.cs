using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Areas.Admin.Controllers;

[Authorize(Roles = "Admin")]
[Area("Admin")]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;
    private readonly IOrderService _orderService;

    public CustomersController(ICustomerService customerService, IOrderService orderService)
    {
        _customerService = customerService;
        _orderService = orderService;
    }

    public async Task<IActionResult> Index(string? search)
    {
        var customers = await _customerService.SearchAsync(search);
        ViewBag.SearchTerm = search;
        return View(customers);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);
        if (customer == null) return NotFound();

        ViewBag.Orders = await _orderService.GetByCustomerAsync(id);
        return View(customer);
    }
}
