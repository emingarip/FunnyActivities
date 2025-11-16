using System;

namespace FunnyActivities.Application.DTOs.PersonaManagement
{
    public class PersonaImageDto
    {
        public Guid Id { get; set; }
        public Guid? PersonaId { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public string ContentType { get; set; }
        public long FileSize { get; set; }
        public string BucketName { get; set; }
        public string ObjectKey { get; set; }
        public string PreSignedUrl { get; set; }
        public string ImageType { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}
