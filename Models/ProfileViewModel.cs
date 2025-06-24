using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class ProfileViewModel
    {
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Họ và tên là bắt buộc")]
        [StringLength(47, ErrorMessage = "Họ và tên phải có độ dài tối đa 47 ký tự.")]
        [RegularExpression(@"^[a-zA-ZÀ-ỹĂăÂâĐđÊêÔôƠơƯư][a-zA-ZÀ-ỹĂăÂâĐđÊêÔôƠơƯư\s]*$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái, không bắt đầu bằng khoảng trắng hoặc ký tự đặc biệt.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Phone]
        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        public string PhoneNumber { get; set; }
    }
}
