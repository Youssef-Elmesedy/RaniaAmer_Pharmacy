using Awlad_Zamzam.MVC.Models.Exceptions;
using Awlad_Zamzam.MVC.Services.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Awlad_Zamzam.MVC.Services.Implementations;

public class ImageService : IImageService
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<ImageService> _logger;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

    // Reject anything bigger than this before even trying to process it - keeps memory/CPU
    // usage predictable and stops the disk filling up with huge raw phone photos.
    private const long MaxUploadBytes = 3 * 1024 * 1024; // 3 MB

    // Longest side, in pixels, that a saved product/category image is ever allowed to be.
    // Product photos never need to be shown larger than this on the site, and it's the single
    // biggest lever for keeping disk usage sane on a small server.
    private const int MaxDimension = 1200;

    // 0-100. 75-82 is the usual "looks the same, file is much smaller" sweet spot for WebP.
    private const int WebpQuality = 80;

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

        if (file.Length <= 0)
            throw new BusinessException("الملف المرفوع فارغ.", nameof(file));

        if (file.Length > MaxUploadBytes)
            throw new BusinessException(
                $"حجم الصورة كبير جدًا. الحد الأقصى المسموح به {MaxUploadBytes / 1024 / 1024} ميجابايت.",
                nameof(file));

        var webRoot = _env.WebRootPath;

        if (string.IsNullOrWhiteSpace(webRoot))
            throw new BusinessException("مجلد wwwroot غير موجود في المشروع، تعذر حفظ الصورة.", nameof(file));

        // Every upload is normalized to .webp regardless of the original format: smaller files
        // on disk, one predictable format to serve, and no surprise-huge originals being kept.
        var fileName = $"{Guid.NewGuid()}.webp";
        var uploadsFolder = Path.Combine(webRoot, "Uplodes", folder);

        try
        {
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            using var inputStream = file.OpenReadStream();
            using var image = await Image.LoadAsync(inputStream);

            // Downscale only — never upscale a smaller image, that only wastes space for no
            // visual benefit.
            if (image.Width > MaxDimension || image.Height > MaxDimension)
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(MaxDimension, MaxDimension)
                }));
            }

            var encoder = new WebpEncoder { Quality = WebpQuality };

            await using var outputStream = new FileStream(filePath, FileMode.Create);
            await image.SaveAsync(outputStream, encoder);

            return $"/Uplodes/{folder}/{fileName}";
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _logger.LogError(ex, "Failed to save uploaded image to {Folder}", uploadsFolder);
            throw new BusinessException("تعذر حفظ الصورة على السيرفر، تأكد من صلاحيات الكتابة على مجلد wwwroot/Uplodes.", nameof(file));
        }
        catch (Exception ex)
        {
            // Covers ImageSharp's own exceptions for corrupt/unrecognized image content — the
            // extension check above only looks at the filename, this is the real content check.
            _logger.LogWarning(ex, "Uploaded file could not be processed as an image: {FileName}", file.FileName);
            throw new BusinessException("تعذر معالجة الملف المرفوع، تأكد من أنه صورة صالحة.", nameof(file));
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
