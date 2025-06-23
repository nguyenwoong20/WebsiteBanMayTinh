using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class ProfileViewModel
    {
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
    }
}
