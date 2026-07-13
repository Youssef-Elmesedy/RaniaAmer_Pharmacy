using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace Awlad_Zamzam.MVC.Services.Implementations;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageService> _logger;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    public ImageService(IWebHostEnvironment env, ILogger<ImageService> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(IFormFile file, string folder)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new BusinessException("صيغة الصورة غير مدعومة. المسموح: jpg, jpeg, png, webp", nameof(file));

        var webRoot = _env.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
            throw new BusinessException("مجلد wwwroot غير موجود في المشروع، تعذر حفظ الصورة.", nameof(file));

        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadsFolder = Path.Combine(webRoot, "Uplodes", folder);

        try
        {
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/Uplodes/{folder}/{fileName}";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Failed to save uploaded image to {Folder}", uploadsFolder);
            throw new BusinessException("تعذر حفظ الصورة على السيرفر، تأكد من صلاحيات الكتابة على مجلد wwwroot/Uplodes.", nameof(file));
        }
    }

    public void DeleteImage(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
            return;

        try
        {
            var fullPath = Path.Combine(_env.WebRootPath, imagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Deleting an old image is best-effort - never let it block the actual operation (update/delete)
            _logger.LogWarning(ex, "Failed to delete image at {ImagePath}", imagePath);
        }
    }
}
