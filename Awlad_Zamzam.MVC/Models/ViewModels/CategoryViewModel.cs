namespace Awlad_Zamzam.MVC.Models.ViewModels;

public class CategoryViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public int ProductsCount { get; set; }
}
