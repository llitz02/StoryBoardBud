namespace StoryBoardBud.Services;

/// <summary>
/// Defines operations for photo file storage
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a photo file to storage
    /// </summary>
    /// <param name="file">Photo file to upload</param>
    /// <param name="userId">ID of user uploading the file</param>
    /// <returns>File path of uploaded photo</returns>
    Task<string> UploadPhotoAsync(IFormFile file, string userId);
    
    /// <summary>
    /// Deletes a photo file from storage
    /// </summary>
    /// <param name="filePath">Path to file to delete</param>
    /// <returns>True if deleted successfully</returns>
    Task<bool> DeletePhotoAsync(string filePath);
    
    /// <summary>
    /// Gets the public URL for a photo file
    /// </summary>
    /// <param name="filePath">Path to file</param>
    /// <returns>Public URL to access the photo</returns>
    string GetPhotoUrl(string filePath);
}
