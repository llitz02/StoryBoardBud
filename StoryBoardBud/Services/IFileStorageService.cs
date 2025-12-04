namespace StoryBoardBud.Services;

public interface IFileStorageService
{
    Task<string> UploadPhotoAsync(IFormFile file, string userId);
    Task<bool> DeletePhotoAsync(string filePath);
    string GetPhotoUrl(string filePath);
}
