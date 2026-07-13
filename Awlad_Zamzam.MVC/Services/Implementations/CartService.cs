using System.Text.Json;
using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Models.ViewModels;
using Awlad_Zamzam.MVC.Repository.Interfaces;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Awlad_Zamzam.MVC.Services.Implementations;

// Cart lines stored server-side in Session (no customer login exists, so a session cart is used)
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
        var items = new List<CartItemViewModel>();

        foreach (var line in lines)
        {
            var product = await _productRepository.GetByIdAsync(line.ProductId);
            if (product == null) continue;

            items.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductImagePath = product.ImagePath,
                UnitPrice = product.DiscountPercentage > 0
                    ? Math.Round(product.Price - (product.Price * product.DiscountPercentage / 100), 2)
                    : product.Price,
                Quantity = line.Quantity,
                Note = line.Note
            });
        }

        return new CartViewModel { Items = items };
    }

    public async Task AddItemAsync(Guid productId, int quantity, string? note)
    {
        if (quantity <= 0)
            throw new BusinessException("الكمية يجب أن تكون أكبر من صفر", nameof(quantity));

        var product = await _productRepository.GetByIdAsync(productId)
            ?? throw new BusinessException("المنتج غير موجود", nameof(productId));

        if (!product.IsAvailable)
            throw new BusinessException("هذا المنتج غير متوفر حالياً", nameof(productId));

        var lines = GetLines();
        var existing = lines.FirstOrDefault(l => l.ProductId == productId);

        if (existing != null)
        {
            existing.Quantity += quantity;
            if (!string.IsNullOrWhiteSpace(note)) existing.Note = note;
        }
        else
        {
            lines.Add(new CartLine { ProductId = productId, Quantity = quantity, Note = note });
        }

        SaveLines(lines);
    }

    public Task UpdateQuantityAsync(Guid productId, int quantity)
    {
        var lines = GetLines();
        var existing = lines.FirstOrDefault(l => l.ProductId == productId);

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

    public Task RemoveItemAsync(Guid productId)
    {
        var lines = GetLines();
        lines.RemoveAll(l => l.ProductId == productId);
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
        public int Quantity { get; set; }
        public string? Note { get; set; }
    }
}
