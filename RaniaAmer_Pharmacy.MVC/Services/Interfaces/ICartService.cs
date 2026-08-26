using RaniaAmer_Pharmacy.MVC.Models.ViewModels;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface ICartService
{
    Task<CartViewModel> GetCartAsync();
    Task AddItemAsync(Guid productId, Guid? saleUnitId, decimal quantity, string? note);
    Task UpdateQuantityAsync(Guid productId, Guid saleUnitId, decimal quantity);
    Task RemoveItemAsync(Guid productId, Guid saleUnitId);
    void Clear();
}
