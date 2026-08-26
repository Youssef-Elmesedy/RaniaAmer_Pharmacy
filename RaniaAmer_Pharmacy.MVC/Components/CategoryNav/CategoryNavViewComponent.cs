using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace RaniaAmer_Pharmacy.MVC.Components.CategoryNav;

// Renders the category dropdown in the main navbar. Rendered on every public page,
// so a transient DB error here must degrade gracefully (empty list) instead of crashing the page.
public class CategoryNavViewComponent : ViewComponent
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryNavViewComponent> _logger;

    public CategoryNavViewComponent(ICategoryService categoryService, ILogger<CategoryNavViewComponent> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        List<CategorySelectItem> categories;

        try
        {
            categories = await _categoryService.GetSelectListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load categories for the navbar.");
            categories = new List<CategorySelectItem>();
        }

        return View(categories);
    }
}
