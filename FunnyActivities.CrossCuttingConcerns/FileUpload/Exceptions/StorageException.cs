using System;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload.Exceptions
{
    public class StorageException : FileUploadException
    {
        public string Operation { get; }
        public string BucketName { get; }
        public string ObjectKey { get; }

        public StorageException(string message)
            : base(message)
        {
        }

        public StorageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public StorageException(string message, string operation, string bucketName, string objectKey)
            : base(message)
        {
            Operation = operation;
            BucketName = bucketName;
            ObjectKey = objectKey;
        }

        public StorageException(string message, string operation, string bucketName, string objectKey, Exception innerException)
            : base(message, innerException)
        {
            Operation = operation;
            BucketName = bucketName;
            ObjectKey = objectKey;
        }
    }
}