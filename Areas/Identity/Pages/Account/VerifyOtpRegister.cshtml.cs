using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Website_BanMayTinh.Models;

namespace Website_BanMayTinh.Areas.Identity.Pages.Account
{
    public class VerifyOtpRegisterModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public VerifyOtpRegisterModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Otp { get; set; }

        public IActionResult OnGet(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData["Message"] = "Không tìm thấy email để xác thực.";
                return RedirectToPage("./Register");
            }

            Email = email;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(Otp))
            {
                TempData["Message"] = "Vui lòng nhập mã OTP hợp lệ.";
                return Page();
            }

            var expectedOtp = HttpContext.Session.GetString("RegisterOtp_" + Email);
            if (Otp != expectedOtp)
            {
                TempData["Message"] = "Mã OTP không chính xác.";
                return Page();
            }

            // Lấy thông tin người dùng từ Session
            var fullName = HttpContext.Session.GetString("Register_FullName");
            var age = HttpContext.Session.GetString("Register_Age");
            var phone = HttpContext.Session.GetString("Register_Phone");
            var password = HttpContext.Session.GetString("Register_Password");
            var role = HttpContext.Session.GetString("Register_Role");

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(password))
            {
                TempData["Message"] = "Không tìm thấy thông tin đăng ký.";
                return RedirectToPage("./Register");
            }

            // Tạo tài khoản
            var user = new ApplicationUser
            {
                FullName = fullName,
                Age = age,
                PhoneNumber = phone,
                UserName = Email,
                Email = Email
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                TempData["Message"] = "Không thể tạo tài khoản: " + string.Join(", ", result.Errors.Select(e => e.Description));
                return RedirectToPage("./Register");
            }

            if (!string.IsNullOrEmpty(role))
                await _userManager.AddToRoleAsync(user, role);
            else
                await _userManager.AddToRoleAsync(user, "Customer");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            await _userManager.ConfirmEmailAsync(user, token);
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Xóa session
            HttpContext.Session.Remove("RegisterOtp_" + Email);
            HttpContext.Session.Remove("Register_FullName");
            HttpContext.Session.Remove("Register_Age");
            HttpContext.Session.Remove("Register_Phone");
            HttpContext.Session.Remove("Register_Password");
            HttpContext.Session.Remove("Register_Role");

            return RedirectToAction("Index", "Home", new { area = "" });
        }
    }
}