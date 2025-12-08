namespace StoryBoardBud.Services;

/// <summary>
/// Handles photo file storage on local filesystem
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<LocalFileStorageService> _logger;
    private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB
    private const string UploadFolder = "uploads";
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    /// <summary>
    /// Initializes the file storage service with web hosting environment
    /// </summary>
    /// <param name="webHostEnvironment">Web hosting environment for file paths</param>
    /// <param name="logger">Logger for recording events</param>
    public LocalFileStorageService(IWebHostEnvironment webHostEnvironment, ILogger<LocalFileStorageService> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a photo to the local uploads folder
    /// </summary>
    /// <param name="file">Photo file to upload</param>
    /// <param name="userId">ID of user uploading the file</param>
    /// <returns>Relative file path of uploaded photo</returns>
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

    /// <summary>
    /// Deletes a photo file from local storage
    /// </summary>
    /// <param name="filePath">Relative path to file to delete</param>
    /// <returns>True if file was deleted</returns>
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

    /// <summary>
    /// Converts file path to public URL
    /// </summary>
    /// <param name="filePath">Relative file path</param>
    /// <returns>Public URL for the photo</returns>
    public string GetPhotoUrl(string filePath)
    {
        return $"/{filePath}";
    }
}
