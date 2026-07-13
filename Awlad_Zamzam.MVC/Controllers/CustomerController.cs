using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Awlad_Zamzam.MVC.Controllers;

// "تواصل معنا" - customers leave their contact details to place an order / inquiry
public class CustomerController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public IActionResult Index() => View(new CustomerViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CustomerViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _customerService.CreateAsync(model);
        }
        catch (BusinessException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        TempData["SuccessMessage"] = "تم إرسال طلبك بنجاح، سنتواصل معك قريباً";
        return RedirectToAction(nameof(Index));
    }
}
