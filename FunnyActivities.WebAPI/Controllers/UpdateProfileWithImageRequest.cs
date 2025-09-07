using Microsoft.AspNetCore.Http;

namespace FunnyActivities.WebAPI.Controllers
{
    public class UpdateProfileWithImageRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public IFormFile? ProfileImage { get; set; }
    }
}