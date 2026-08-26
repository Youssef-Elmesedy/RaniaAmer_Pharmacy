using RaniaAmer_Pharmacy.MVC.Models.Exceptions;

namespace RaniaAmer_Pharmacy.MVC.Models.Entities;

// A unit a product can be sold by (قطعة، علبة، شريط، قرص...). Fully managed by the admin
// (add/edit/delete) instead of being a fixed list, so the pharmacy can adapt it as needed.
public class SaleUnit : BaseEntity
{
    public string Name { get; private set; } = string.Empty;

    public string NormalizedName { get; private set; } = string.Empty;

    public ICollection<Product> Products { get; private set; }
        = new List<Product>();

    private SaleUnit()
    {
    }

    public static SaleUnit Create(string name)
    {
        Validate(name);

        return new SaleUnit
        {
            Name = name.Trim(),
            NormalizedName = name.Trim().ToUpperInvariant(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name)
    {
        Validate(name);

        Name = name.Trim();
        NormalizedName = name.Trim().ToUpperInvariant();

        UpdatedAt = DateTime.UtcNow;
    }

    private static void Validate(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new BusinessException("اسم وحدة البيع مطلوب", nameof(name));

        if (name.Length > 30)
            throw new BusinessException("اسم وحدة البيع لا يجب أن يتجاوز 30 حرف", nameof(name));
    }
}
