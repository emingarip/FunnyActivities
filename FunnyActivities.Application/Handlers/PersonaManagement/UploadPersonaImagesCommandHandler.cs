using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FunnyActivities.Application.Commands.PersonaManagement;
using FunnyActivities.Application.DTOs.PersonaManagement;
using FunnyActivities.Application.Interfaces;
using FunnyActivities.Domain.Entities;
using FunnyActivities.Domain.Interfaces;

namespace FunnyActivities.Application.Handlers.PersonaManagement
{
    public class UploadPersonaImagesCommandHandler : IRequestHandler<UploadPersonaImagesCommand, List<PersonaImageDto>>
    {
        private readonly IImageProcessingService _imageProcessingService;
        private readonly IMinioService _minioService;
        private readonly IPersonaRepository _personaRepository;

        private const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB limit

        public UploadPersonaImagesCommandHandler(
            IImageProcessingService imageProcessingService,
            IMinioService minioService,
            IPersonaRepository personaRepository)
        {
            _imageProcessingService = imageProcessingService;
            _minioService = minioService;
            _personaRepository = personaRepository;
        }

        public async Task<List<PersonaImageDto>> Handle(UploadPersonaImagesCommand request, CancellationToken cancellationToken)
        {
            var persona = await _personaRepository.GetByIdAsync(request.PersonaId);
            if (persona == null)
            {
                throw new KeyNotFoundException("Persona not found.");
            }

            if (persona.UserId != request.UserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to upload images for this persona.");
            }

            var uploadedImages = new List<PersonaImageDto>();

            foreach (var file in request.Files)
            {
                if (file.Data == null || file.Data.Length == 0)
                {
                    throw new ArgumentException("Boş bir dosya gönderildi.");
                }

                if (file.Data.Length > MaxImageSizeBytes)
                {
                    throw new ArgumentException("Görüntü dosyası 5MB sınırını aşıyor.");
                }

                if (!await _imageProcessingService.ValidateImageAsync(file.Data, file.ContentType))
                {
                    throw new ArgumentException("Geçersiz dosya. Sadece JPEG ve PNG destekleniyor.");
                }

                // Hazırlanan versiyonlar
                var thumbnailData = await _imageProcessingService.ResizeImageAsync(file.Data, 150, 150);
                var mediumData = await _imageProcessingService.ResizeImageAsync(file.Data, 500, 500);

                // MinIO'ya yükle
                var originalKey = await _minioService.UploadPersonaImageAsync(file.Data, file.FileName, file.ContentType, request.PersonaId, "original");
                var thumbKey = await _minioService.UploadPersonaImageAsync(thumbnailData, $"thumb_{file.FileName}", file.ContentType, request.PersonaId, "thumbnail");
                var mediumKey = await _minioService.UploadPersonaImageAsync(mediumData, $"medium_{file.FileName}", file.ContentType, request.PersonaId, "medium");

                // Pre-signed URL üret
                var originalUrl = await _minioService.GeneratePersonaPreSignedUrlAsync(originalKey);
                var thumbUrl = await _minioService.GeneratePersonaPreSignedUrlAsync(thumbKey);
                var mediumUrl = await _minioService.GeneratePersonaPreSignedUrlAsync(mediumKey);

                // Metadata kaydet
                var originalImage = new Image(
                    Guid.NewGuid(),
                    request.UserId,
                    request.PersonaId,
                    file.FileName,
                    file.FileName,
                    file.ContentType,
                    file.Data.Length,
                    "persona-images",
                    originalKey,
                    originalUrl,
                    "original");

                var thumbImage = new Image(
                    Guid.NewGuid(),
                    request.UserId,
                    request.PersonaId,
                    $"thumb_{file.FileName}",
                    file.FileName,
                    file.ContentType,
                    thumbnailData.Length,
                    "persona-images",
                    thumbKey,
                    thumbUrl,
                    "thumbnail");

                var mediumImage = new Image(
                    Guid.NewGuid(),
                    request.UserId,
                    request.PersonaId,
                    $"medium_{file.FileName}",
                    file.FileName,
                    file.ContentType,
                    mediumData.Length,
                    "persona-images",
                    mediumKey,
                    mediumUrl,
                    "medium");

                await _minioService.SaveImageMetadataAsync(originalImage);
                await _minioService.SaveImageMetadataAsync(thumbImage);
                await _minioService.SaveImageMetadataAsync(mediumImage);

                // Primary avatar'ı güncelle (ilk yüklenen görselin orijinali)
                if (string.IsNullOrWhiteSpace(persona.AvatarImageUrl))
                {
                    persona.UpdateDetails(persona.Name, persona.Description, originalUrl, persona.Age, persona.Gender, persona.Nationality, persona.Biography);
                    await _personaRepository.UpdateAsync(persona);
                }

                uploadedImages.Add(new PersonaImageDto
                {
                    Id = originalImage.Id,
                    PersonaId = request.PersonaId,
                    FileName = originalImage.FileName,
                    OriginalFileName = originalImage.OriginalFileName,
                    ContentType = originalImage.ContentType,
                    FileSize = originalImage.FileSize,
                    BucketName = originalImage.BucketName,
                    ObjectKey = originalImage.ObjectKey,
                    PreSignedUrl = originalImage.PreSignedUrl,
                    ImageType = originalImage.ImageType,
                    UploadedAt = originalImage.UploadedAt
                });
            }

            return uploadedImages;
        }
    }
}
