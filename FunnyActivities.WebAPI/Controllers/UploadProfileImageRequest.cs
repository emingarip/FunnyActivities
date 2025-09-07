using Microsoft.AspNetCore.Http;

namespace FunnyActivities.WebAPI.Controllers
{
    public class UploadProfileImageRequest
    {
        public IFormFile ImageFile { get; set; }
    }
}