using System;
using System.Collections.Generic;

namespace FunnyActivities.Domain.ValueObjects
{
    /// <summary>
    /// Represents metadata information for MinIO objects.
    /// </summary>
    public class ObjectMetadata
    {
        public string ObjectKey { get; set; }
        public string BucketName { get; set; }
        public long Size { get; set; }
        public string ContentType { get; set; }
        public DateTime LastModified { get; set; }
        public string ETag { get; set; }
        public Dictionary<string, string> UserMetadata { get; set; } = new Dictionary<string, string>();
    }
}