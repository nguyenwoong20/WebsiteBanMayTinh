using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Website_BanMayTinh.Models;

namespace Website_BanMayTinh.Areas.Identity.Pages.Account
{
    public class VerifyOtpModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public VerifyOtpModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mã OTP.")]
            [Display(Name = "Mã OTP")]
            public string OTP { get; set; }
        }

        public IActionResult OnGet()
        {
            if (TempData["ResetOTP"] == null || TempData["ResetEmail"] == null)
            {
                return RedirectToPage("./ForgotPassword");
            }

            // Lưu lại TempData để không bị mất sau Redirect
            TempData.Keep("ResetOTP");
            TempData.Keep("ResetEmail");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var otpSession = TempData["ResetOTP"]?.ToString();
            var email = TempData["ResetEmail"]?.ToString();

            if (otpSession == null || email == null)
                return RedirectToPage("./ForgotPassword");

            if (Input.OTP != otpSession)
            {
                ModelState.AddModelError(string.Empty, "Mã OTP không chính xác.");
                TempData.Keep("ResetOTP");
                TempData.Keep("ResetEmail");
                return Page();
            }

            // OTP đúng → chuyển đến trang ResetPassword
            var user = await _userManager.FindByEmailAsync(email);
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            return RedirectToPage("ResetPassword", new { code = encodedCode, email });

        }
    }
}