using System.ComponentModel.DataAnnotations;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload
{
    public class FileUploadRequest
    {
        [Required]
        public string FileName { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;

        [Required]
        public byte[] FileData { get; set; } = Array.Empty<byte>();

        public string BucketName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class FileUploadResult
    {
        public bool Success { get; set; }
        public string FileId { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public FileMetadata Metadata { get; set; } = new();
    }

    public class FileMetadata
    {
        public string FileId { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string BucketName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
        public Dictionary<string, string> CustomMetadata { get; set; } = new();
    }

    public class FileValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}