namespace FunnyActivities.CrossCuttingConcerns.FileUpload.Configuration
{
    public class FileTypeConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public List<string> AllowedContentTypes { get; set; } = new();
        public List<string> AllowedExtensions { get; set; } = new();
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB default
        public long MinFileSize { get; set; } = 0;
        public bool EnableProcessing { get; set; } = true;
        public Dictionary<string, string> ProcessingOptions { get; set; } = new();
        public bool EnableThumbnail { get; set; } = false;
        public int ThumbnailWidth { get; set; } = 200;
        public int ThumbnailHeight { get; set; } = 200;
        public bool EnableCompression { get; set; } = false;
        public int CompressionQuality { get; set; } = 80;
        public string DefaultBucket { get; set; } = string.Empty;
        public bool RequireAuthentication { get; set; } = true;
        public List<string> AllowedRoles { get; set; } = new();
    }
}