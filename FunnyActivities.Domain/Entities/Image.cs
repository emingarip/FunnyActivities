using System;

namespace FunnyActivities.Domain.Entities
{
    public class Image
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public Guid? PersonaId { get; private set; }
        public string FileName { get; private set; }
        public string OriginalFileName { get; private set; }
        public string ContentType { get; private set; }
        public long FileSize { get; private set; }
        public string BucketName { get; private set; }
        public string ObjectKey { get; private set; }
        public string PreSignedUrl { get; private set; }
        public DateTime UploadedAt { get; private set; }
        public string ImageType { get; private set; } // "thumbnail", "medium", "original"

        public Image(Guid id, Guid userId, Guid? personaId, string fileName, string originalFileName, string contentType,
                    long fileSize, string bucketName, string objectKey, string preSignedUrl, string imageType)
        {
            Id = id;
            UserId = userId;
            PersonaId = personaId;
            FileName = fileName;
            OriginalFileName = originalFileName;
            ContentType = contentType;
            FileSize = fileSize;
            BucketName = bucketName;
            ObjectKey = objectKey;
            PreSignedUrl = preSignedUrl;
            ImageType = imageType;
            UploadedAt = DateTime.UtcNow;
        }

        // Private constructor for EF
        private Image() { }
    }
}
