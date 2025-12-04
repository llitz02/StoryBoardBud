namespace StoryBoardBud.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private const string UploadFolder = "uploads";
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public LocalFileStorageService(IWebHostEnvironment webHostEnvironment, ILogger<LocalFileStorageService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public async Task<string> UploadPhotoAsync(IFormFile file, string userId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxFileSize)
            throw new ArgumentException($"File size exceeds the maximum limit of {MaxFileSize / (1024 * 1024)} MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            throw new ArgumentException("File type is not allowed.");

        try
        {
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, UploadFolder);
            Directory.CreateDirectory(uploadsPath);

            var uniqueFileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            _logger.LogInformation($"Photo uploaded successfully: {uniqueFileName}");
            return Path.Combine(UploadFolder, uniqueFileName).Replace("\\", "/");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error uploading photo: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> DeletePhotoAsync(string filePath)
    {
        try
        {
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, filePath);
            
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation($"Photo deleted: {filePath}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error deleting photo: {ex.Message}");
            throw;
        }
    }

    public string GetPhotoUrl(string filePath)
    {
        return $"/{filePath}";
    }
}
