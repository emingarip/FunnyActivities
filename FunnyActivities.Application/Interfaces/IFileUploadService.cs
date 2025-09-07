using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FunnyActivities.Application.Interfaces
{
    /// <summary>
    /// Interface for file upload operations.
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Uploads multiple files for a material.
        /// </summary>
        /// <param name="materialId">The material ID.</param>
        /// <param name="photos">The photos to upload.</param>
        /// <param name="userId">The user ID performing the upload.</param>
        /// <returns>A task representing the upload operation.</returns>
        Task<UploadPhotosResult> UploadMaterialPhotosAsync(Guid materialId, IEnumerable<IFormFile> photos, Guid userId);

        /// <summary>
        /// Deletes a file by its ID.
        /// </summary>
        /// <param name="fileId">The file ID to delete.</param>
        /// <returns>True if deleted successfully; otherwise, false.</returns>
        Task<bool> DeleteFileAsync(string fileId);

        /// <summary>
        /// Gets the URL for a file.
        /// </summary>
        /// <param name="fileId">The file ID.</param>
        /// <param name="expiryInSeconds">Expiry time in seconds.</param>
        /// <returns>The file URL.</returns>
        Task<string> GetFileUrlAsync(string fileId, int expiryInSeconds = 3600);
    }

    /// <summary>
    /// Result of uploading photos.
    /// </summary>
    public class UploadPhotosResult
    {
        /// <summary>
        /// Gets or sets the uploaded photo URLs.
        /// </summary>
        public List<string> PhotoUrls { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets the upload errors.
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a value indicating whether the upload was successful.
        /// </summary>
        public bool Success => Errors.Count == 0;
    }
}