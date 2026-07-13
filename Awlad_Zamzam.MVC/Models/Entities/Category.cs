using Awlad_Zamzam.MVC.Models.Exceptions;

namespace Awlad_Zamzam.MVC.Models.Entities;

public class Category : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public string Image { get; private set; } = string.Empty;

    public ICollection<Product> Products { get; private set; }
        = new List<Product>();

    private Category()
    {
    }

    public static Category Create(string name, string image)
    {
        Validate(name, image);

        return new Category
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            Image = image.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string image)
    {
        Validate(name, image);

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();
        Image = image.Trim();

        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name, string image)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("Category name cannot be empty.", nameof(name));

        if (name.Length > 100)
            throw new BusinessException("Category name cannot exceed 100 characters.", paramName: nameof(name));

        if (string.IsNullOrWhiteSpace(image))
            throw new BusinessException("Category image is required.", nameof(image));
    }
}