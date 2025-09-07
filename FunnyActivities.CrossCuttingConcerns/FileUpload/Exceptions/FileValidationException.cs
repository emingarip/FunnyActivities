using System;
using System.Collections.Generic;

namespace FunnyActivities.CrossCuttingConcerns.FileUpload.Exceptions
{
    public class FileValidationException : FileUploadException
    {
        public List<string> ValidationErrors { get; }

        public FileValidationException(string message)
            : base(message)
        {
            ValidationErrors = new List<string> { message };
        }

        public FileValidationException(List<string> errors)
            : base(string.Join("; ", errors))
        {
            ValidationErrors = errors;
        }

        public FileValidationException(string message, string fileName, string contentType, long fileSize, string bucketName)
            : base(message, fileName, contentType, fileSize, bucketName)
        {
            ValidationErrors = new List<string> { message };
        }
    }
}