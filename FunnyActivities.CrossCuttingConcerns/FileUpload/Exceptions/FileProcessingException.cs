using System;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload.Exceptions
{
    public class FileProcessingException : FileUploadException
    {
        public string ProcessingOperation { get; }

        public FileProcessingException(string message)
            : base(message)
        {
        }

        public FileProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public FileProcessingException(string message, string processingOperation)
            : base(message)
        {
            ProcessingOperation = processingOperation;
        }

        public FileProcessingException(string message, string processingOperation, Exception innerException)
            : base(message, innerException)
        {
            ProcessingOperation = processingOperation;
        }

        public FileProcessingException(string message, string fileName, string contentType, long fileSize, string bucketName, string processingOperation)
            : base(message, fileName, contentType, fileSize, bucketName)
        {
            ProcessingOperation = processingOperation;
        }
    }
}