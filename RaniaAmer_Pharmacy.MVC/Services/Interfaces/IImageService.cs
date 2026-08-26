using Microsoft.AspNetCore.Http;

namespace RaniaAmer_Pharmacy.MVC.Services.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder);
    void DeleteImage(string? imagePath);
}
