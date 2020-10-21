using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ChannelPartnerApplication.Models.RequestModels
{
    public class CommonRegistrationModel
    {
        public List<IFormFile> Files { get; set; }
        public List<IFormFile> Video { get; set; }
        public IFormFile File { get; set; }
        public string Data { get; set; }

        [Required]
        public string DeviceId { get; set; }
        [Required]
        public string FCMId { get; set; }
    }
}
