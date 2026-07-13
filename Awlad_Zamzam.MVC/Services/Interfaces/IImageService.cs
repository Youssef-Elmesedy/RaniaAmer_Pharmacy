using Microsoft.AspNetCore.Http;

namespace Awlad_Zamzam.MVC.Services.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(IFormFile file, string folder);
    void DeleteImage(string? imagePath);
}
