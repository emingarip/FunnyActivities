namespace FunnyActivities.CrossCuttingConcerns.FileUpload.Configuration
{
    public class BucketConfiguration
    {
        public string Name { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public bool EnableVersioning { get; set; } = false;
        public bool EnableEncryption { get; set; } = false;
        public long MaxBucketSize { get; set; } = -1; // -1 means unlimited
        public int MaxObjects { get; set; } = -1; // -1 means unlimited
        public List<string> AllowedFileTypes { get; set; } = new();
        public bool IsPublic { get; set; } = false;
        public string LifecyclePolicy { get; set; } = string.Empty;
        public Dictionary<string, string> Tags { get; set; } = new();
    }
}