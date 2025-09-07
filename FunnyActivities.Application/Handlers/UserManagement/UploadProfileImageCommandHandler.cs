using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.UserManagement;
using FunnyActivities.Application.DTOs.UserManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Interfaces;
using FunnyActivities.Domain.Entities;

namespace FunnyActivities.Application.Handlers
{
    public class UploadProfileImageCommandHandler : IRequestHandler<UploadProfileImageCommand, UploadProfileImageResponse>
    {
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IMinioService _minioService;
        private readonly FunnyActivities.Domain.Interfaces.IUserRepository _userRepository;

        public UploadProfileImageCommandHandler(
            IImageProcessingService imageProcessingService,
            IMinioService minioService,
            FunnyActivities.Domain.Interfaces.IUserRepository userRepository)
        {
            _imageProcessingService = imageProcessingService;
            _minioService = minioService;
            _userRepository = userRepository;
        }

        public async Task<UploadProfileImageResponse> Handle(UploadProfileImageCommand request, CancellationToken cancellationToken)
        {
            // Validate image
            if (!await _imageProcessingService.ValidateImageAsync(request.ImageData, request.ContentType))
            {
                throw new ArgumentException("Invalid image file. Only JPEG and PNG formats are supported.");
            }

            // Check file size (5MB limit)
            if (request.ImageData.Length > 5 * 1024 * 1024)
            {
                throw new ArgumentException("Image file size exceeds 5MB limit.");
            }

            // Process images: create thumbnail, medium, and keep original
            var thumbnailData = await _imageProcessingService.ResizeImageAsync(request.ImageData, 150, 150);
            var mediumData = await _imageProcessingService.ResizeImageAsync(request.ImageData, 500, 500);

            // Upload images to MinIO
            var originalObjectKey = await _minioService.UploadImageAsync(request.ImageData, request.FileName, request.ContentType, "original");
            var thumbnailObjectKey = await _minioService.UploadImageAsync(thumbnailData, $"thumb_{request.FileName}", request.ContentType, "thumbnail");
            var mediumObjectKey = await _minioService.UploadImageAsync(mediumData, $"medium_{request.FileName}", request.ContentType, "medium");

            // Generate pre-signed URLs
            var originalUrl = await _minioService.GeneratePreSignedUrlAsync(originalObjectKey);
            var thumbnailUrl = await _minioService.GeneratePreSignedUrlAsync(thumbnailObjectKey);
            var mediumUrl = await _minioService.GeneratePreSignedUrlAsync(mediumObjectKey);

            // Save metadata to database
            var originalImage = new Image(
                Guid.NewGuid(),
                request.UserId,
                request.FileName,
                request.FileName,
                request.ContentType,
                request.ImageData.Length,
                "profile-images",
                originalObjectKey,
                originalUrl,
                "original"
            );

            var thumbnailImage = new Image(
                Guid.NewGuid(),
                request.UserId,
                $"thumb_{request.FileName}",
                request.FileName,
                request.ContentType,
                thumbnailData.Length,
                "profile-images",
                thumbnailObjectKey,
                thumbnailUrl,
                "thumbnail"
            );

            var mediumImage = new Image(
                Guid.NewGuid(),
                request.UserId,
                $"medium_{request.FileName}",
                request.FileName,
                request.ContentType,
                mediumData.Length,
                "profile-images",
                mediumObjectKey,
                mediumUrl,
                "medium"
            );

            await _minioService.SaveImageMetadataAsync(originalImage);
            await _minioService.SaveImageMetadataAsync(thumbnailImage);
            await _minioService.SaveImageMetadataAsync(mediumImage);

            // Update user's profile image URL
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user != null)
            {
                user.UpdateProfile(user.FirstName, user.LastName, originalUrl);
                await _userRepository.UpdateAsync(user);
            }

            return new UploadProfileImageResponse
            {
                ProfileImageUrl = originalUrl,
                ThumbnailUrl = thumbnailUrl,
                MediumUrl = mediumUrl,
                Message = "Profile image uploaded successfully."
            };
        }
    }
}