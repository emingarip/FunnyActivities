using System;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload.Exceptions
{
    public class FileUploadException : Exception
    {
        public string FileName { get; }
        public string ContentType { get; }
        public long FileSize { get; }
        public string BucketName { get; }

        public FileUploadException(string message)
            : base(message)
        {
        }

        public FileUploadException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public FileUploadException(string message, string fileName, string contentType, long fileSize, string bucketName)
            : base(message)
        {
            FileName = fileName;
            ContentType = contentType;
            FileSize = fileSize;
            BucketName = bucketName;
        }
    }
}