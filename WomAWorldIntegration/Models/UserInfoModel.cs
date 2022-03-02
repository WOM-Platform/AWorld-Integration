using System.ComponentModel.DataAnnotations;

namespace WomAWorldIntegration.Models
{
    public class UserInfoModel
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Secret { get; set; }
    }
}
