using System.Collections.Generic;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload.Configuration
{
    public class FileUploadConfiguration
    {
        public string DefaultBucket { get; set; } = "default";
        public long MaxFileSize { get; set; } = 100 * 1024 * 1024; // 100MB for video support
        public int MaxFilesPerUpload { get; set; } = 10;
        public List<string> AllowedFileTypes { get; set; } = new() { "image/jpeg", "image/png", "image/gif", "image/webp", "video/mp4", "video/avi", "video/mov", "video/wmv", "video/flv", "video/webm" };
        public Dictionary<string, FileTypeConfiguration> FileTypeConfigurations { get; set; } = new();
        public Dictionary<string, BucketConfiguration> BucketConfigurations { get; set; } = new()
        {
            ["activity-videos"] = new BucketConfiguration
            {
                Name = "activity-videos",
                Region = "us-east-1",
                EnableVersioning = false,
                EnableEncryption = false,
                MaxBucketSize = 1073741824, // 1GB
                MaxObjects = -1, // unlimited
                AllowedFileTypes = new List<string> { "video/mp4", "video/avi", "video/mov", "video/wmv", "video/flv", "video/webm" },
                IsPublic = false,
                LifecyclePolicy = string.Empty,
                Tags = new Dictionary<string, string> { ["Purpose"] = "Activity Videos", ["Environment"] = "Production" }
            }
        };
        public bool EnableFileProcessing { get; set; } = true;
        public bool EnableVirusScanning { get; set; } = false;
        public string TempDirectory { get; set; } = Path.GetTempPath();
        public int PreSignedUrlExpirySeconds { get; set; } = 3600;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(DefaultBucket))
                throw new InvalidOperationException("FileUpload configuration error: DefaultBucket is required and cannot be empty or whitespace.");
    
            if (MaxFileSize <= 0)
                throw new InvalidOperationException("FileUpload configuration error: MaxFileSize must be greater than 0.");
    
            if (MaxFilesPerUpload <= 0)
                throw new InvalidOperationException("FileUpload configuration error: MaxFilesPerUpload must be greater than 0.");
    
            if (AllowedFileTypes == null || !AllowedFileTypes.Any())
                throw new InvalidOperationException("FileUpload configuration error: AllowedFileTypes must contain at least one file type.");
    
            if (string.IsNullOrWhiteSpace(TempDirectory))
                throw new InvalidOperationException("FileUpload configuration error: TempDirectory is required and cannot be empty or whitespace.");
    
            if (!Path.IsPathRooted(TempDirectory))
                throw new InvalidOperationException("FileUpload configuration error: TempDirectory must be an absolute path.");
    
            if (PreSignedUrlExpirySeconds <= 0)
                throw new InvalidOperationException("FileUpload configuration error: PreSignedUrlExpirySeconds must be greater than 0.");
    
            if (FileTypeConfigurations == null)
                throw new InvalidOperationException("FileUpload configuration error: FileTypeConfigurations cannot be null.");
    
            if (BucketConfigurations == null)
                throw new InvalidOperationException("FileUpload configuration error: BucketConfigurations cannot be null.");
        }
    }
}