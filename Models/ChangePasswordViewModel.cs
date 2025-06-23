using System.ComponentModel.DataAnnotations;

namespace Website_BanMayTinh.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Bạn chưa nhập mật khẩu hiện tại")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Bạn chưa nhập mật khẩu mới")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.", MinimumLength = 6)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu mới")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
