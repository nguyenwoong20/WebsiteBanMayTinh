using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Website_BanMayTinh.Models;
using Website_BanMayTinh.Repositories;

namespace Website_BanMayTinh.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ICustomEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context, ICustomEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _emailSender = emailSender;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.Remove("cart"); // Xoá session giỏ hàng
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);

            ViewBag.Categories = await _context.Categories.ToListAsync();

            var model = new ProfileViewModel
            {
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                ViewBag.Success = "Thông tin đã được cập nhật.";
                return View(model);
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            var isValid = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
            if (!isValid)
            {
                ModelState.AddModelError("", "Mật khẩu hiện tại không đúng.");
                return View(model);
            }

            // Tạo mã OTP (4-6 chữ số)
            var otpCode = new Random().Next(100000, 999999).ToString();

            // Lưu tạm vào session
            HttpContext.Session.SetString("OTP", otpCode);
            HttpContext.Session.SetString("NewPassword", model.NewPassword);
            HttpContext.Session.SetString("UserId", user.Id);

            // Gửi OTP qua email
            var subject = "Xác nhận đổi mật khẩu (OTP)";
            var body = $@"
        <h3>Xin chào {user.FullName},</h3>
        <p>Mã OTP để xác nhận đổi mật khẩu là: <b>{otpCode}</b></p>
        <p>Mã này có hiệu lực trong 5 phút.</p>
        <p>Trân trọng,<br/>Website Bán Máy Tính</p>
    ";

            await _emailSender.SendEmailAsync(user.Email, subject, body);

            return RedirectToAction("VerifyOtp");
        }

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            return View(new VerifyOtpViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(VerifyOtpViewModel model)
        {
            var otpInSession = HttpContext.Session.GetString("OTP");
            var newPassword = HttpContext.Session.GetString("NewPassword");
            var userId = HttpContext.Session.GetString("UserId");

            if (model.OtpCode != otpInSession)
            {
                ModelState.AddModelError("", "Mã OTP không đúng.");
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(userId);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                // Xoá session tạm
                HttpContext.Session.Remove("OTP");
                HttpContext.Session.Remove("NewPassword");
                HttpContext.Session.Remove("UserId");

                ViewBag.Success = "Mật khẩu đã được đổi thành công.";
                return RedirectToAction("ChangePassword");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }
    }
}
