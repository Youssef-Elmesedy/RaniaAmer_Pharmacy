using System.Text.Json;
using RaniaAmer_Pharmacy.MVC.Models.Exceptions;
using RaniaAmer_Pharmacy.MVC.Models.ViewModels;
using RaniaAmer_Pharmacy.MVC.Repository.Interfaces;
using RaniaAmer_Pharmacy.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace RaniaAmer_Pharmacy.MVC.Services.Implementations;

// Cart lines stored server-side in Session (no customer login exists, so a session cart is used).
// A line is identified by (ProductId, SaleUnitId) — the same product bought in two different
// units (e.g. one box + a few loose strips) is two separate lines, since each has its own price.
//
// SaleUnitId always stores the ACTUAL resolved unit id (the product's base unit id when no
// sub-unit was chosen) — never null — so what's stored, what's displayed, and what's posted
// back from the cart page always match exactly for updates/removals.
public class CartService : ICartService
{
    private const string SessionKey = "Cart";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IProductRepository _productRepository;

    public CartService(IHttpContextAccessor httpContextAccessor, IProductRepository productRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _productRepository = productRepository;
    }

    private ISession Session =>
        _httpContextAccessor.HttpContext?.Session
        ?? throw new InvalidOperationException("Session is not available.");

    public async Task<CartViewModel> GetCartAsync()
    {
        var lines = GetLines();
        if (lines.Count == 0) return new CartViewModel();

        // One round-trip for every product in the cart instead of one per line (this used to
        // run on every single page load via the navbar cart badge — see GetItemsCountAsync).
        var products = await _productRepository.GetByIdsWithDetailsAsync(lines.Select(l => l.ProductId));
        var productsById = products.ToDictionary(p => p.Id);

        var items = new List<CartItemViewModel>();

        foreach (var line in lines)
        {
            if (!productsById.TryGetValue(line.ProductId, out var product)) continue;

            var basePrice = product.DiscountPercentage > 0
                ? Math.Round(product.Price - (product.Price * product.DiscountPercentage / 100), 2)
                : product.Price;

            string saleUnitName;
            decimal unitPrice;

            if (line.SaleUnitId == product.SaleUnitId)
            {
                saleUnitName = product.SaleUnit?.Name ?? string.Empty;
                unitPrice = basePrice;
            }
            else
            {
                var option = product.UnitOptions.FirstOrDefault(o => o.SaleUnitId == line.SaleUnitId);
                if (option == null) continue; // the admin removed this sub-unit since it was added to the cart

                saleUnitName = option.SaleUnit?.Name ?? string.Empty;
                unitPrice = Math.Round(basePrice / option.QuantityPerBaseUnit, 2);
            }

            items.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImagePath = product.ImagePath,
                SaleUnitId = line.SaleUnitId,
                SaleUnitName = saleUnitName,
                UnitPrice = unitPrice,
                Quantity = line.Quantity,
                Note = line.Note
            });
        }

        return new CartViewModel { Items = items };
    }

    // Used by the navbar cart badge (rendered on every public page) — reads the count straight
    // from the session with NO database round-trip at all, instead of loading full cart/pricing
    // details just to count lines.
    public Task<int> GetItemsCountAsync() => Task.FromResult(GetLines().Count);

    public async Task AddItemAsync(Guid productId, Guid? saleUnitId, decimal quantity, string? note)
    {
        if (quantity <= 0)
            throw new BusinessException("الكمية يجب أن تكون أكبر من صفر", nameof(quantity));

        if (quantity != Math.Floor(quantity))
            throw new BusinessException("الكمية يجب أن تكون رقم صحيح", nameof(quantity));

        var product = await _productRepository.GetByIdWithDetailsAsync(productId)
            ?? throw new BusinessException("المنتج غير موجود", nameof(productId));

        if (!product.IsAvailable)
            throw new BusinessException("هذا المنتج غير متوفر حالياً", nameof(productId));

        // Resolve to the actual unit id: no selection (or the base unit itself) means the
        // product's own base unit.
        var resolvedSaleUnitId = (saleUnitId == null || saleUnitId == product.SaleUnitId)
            ? product.SaleUnitId
            : saleUnitId.Value;

        if (resolvedSaleUnitId != product.SaleUnitId && product.UnitOptions.All(o => o.SaleUnitId != resolvedSaleUnitId))
            throw new BusinessException("وحدة البيع المختارة غير متاحة لهذا المنتج", nameof(saleUnitId));

        var lines = GetLines();
        var existing = lines.FirstOrDefault(l => l.ProductId == productId && l.SaleUnitId == resolvedSaleUnitId);

        if (existing != null)
        {
            existing.Quantity += quantity;
            if (!string.IsNullOrWhiteSpace(note)) existing.Note = note;
        }
        else
        {
            lines.Add(new CartLine { ProductId = productId, SaleUnitId = resolvedSaleUnitId, Quantity = quantity, Note = note });
        }

        SaveLines(lines);
    }

    public Task UpdateQuantityAsync(Guid productId, Guid saleUnitId, decimal quantity)
    {
        var lines = GetLines();
        var existing = lines.FirstOrDefault(l => l.ProductId == productId && l.SaleUnitId == saleUnitId);

        if (existing != null)
        {
            if (quantity <= 0)
                lines.Remove(existing);
            else
                existing.Quantity = quantity;

            SaveLines(lines);
        }

        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(Guid productId, Guid saleUnitId)
    {
        var lines = GetLines();
        lines.RemoveAll(l => l.ProductId == productId && l.SaleUnitId == saleUnitId);
        SaveLines(lines);
        return Task.CompletedTask;
    }

    public void Clear() => Session.Remove(SessionKey);

    private List<CartLine> GetLines()
    {
        var json = Session.GetString(SessionKey);
        if (string.IsNullOrWhiteSpace(json)) return new List<CartLine>();

        return JsonSerializer.Deserialize<List<CartLine>>(json) ?? new List<CartLine>();
    }

    private void SaveLines(List<CartLine> lines) =>
        Session.SetString(SessionKey, JsonSerializer.Serialize(lines));

    private class CartLine
    {
        public Guid ProductId { get; set; }
        public Guid SaleUnitId { get; set; }
        public decimal Quantity { get; set; }
        public string? Note { get; set; }
    }
}
