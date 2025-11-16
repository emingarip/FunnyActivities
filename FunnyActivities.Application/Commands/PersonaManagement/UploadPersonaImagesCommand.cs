using MediatR;
using FunnyActivities.Application.DTOs.PersonaManagement;

namespace FunnyActivities.Application.Commands.PersonaManagement
{
    public class UploadPersonaImagesCommand : IRequest<List<PersonaImageDto>>
    {
        public Guid PersonaId { get; set; }
        public Guid UserId { get; set; }
        public List<UploadPersonaImageFile> Files { get; set; } = new();
    }

    public class UploadPersonaImageFile
    {
        public byte[] Data { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}
