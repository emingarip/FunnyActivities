using MediatR;
using FunnyActivities.Application.DTOs.UserManagement;

namespace FunnyActivities.Application.Commands.UserManagement
{
    public class UploadProfileImageCommand : IRequest<UploadProfileImageResponse>
    {
        public Guid UserId { get; set; }
        public byte[] ImageData { get; set; }
        public string FileName { get; set; }
        public string ContentType { get; set; }
    }
}