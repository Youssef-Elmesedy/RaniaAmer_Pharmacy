using Awlad_Zamzam.MVC.Models.ViewModels;

namespace Awlad_Zamzam.MVC.Services.Interfaces;

public interface ICartService
{
    Task<CartViewModel> GetCartAsync();
    Task AddItemAsync(Guid productId, decimal quantity, string? note);
    Task UpdateQuantityAsync(Guid productId, decimal quantity);
    Task RemoveItemAsync(Guid productId);
    void Clear();
}
