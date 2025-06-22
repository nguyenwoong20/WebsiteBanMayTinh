// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Website_BanMayTinh.Models;
using Website_BanMayTinh.Repositories;



namespace Website_BanMayTinh.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly Repositories.ICustomEmailSender _emailSender;

        public ForgotPasswordModel(UserManager<ApplicationUser> userManager, Repositories.ICustomEmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            public string Email { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            var user = await _userManager.FindByEmailAsync(Input.Email);
            //if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không thể gửi OTP. Vui lòng thử lại.";
                return RedirectToPage("./ForgotPassword");
            }

            var otpCode = new Random().Next(100000, 999999).ToString();
            TempData["ResetOTP"] = otpCode;
            TempData["ResetEmail"] = Input.Email;

            try
            {
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Mã OTP đặt lại mật khẩu",
                    $"Mã OTP của bạn là: <strong>{otpCode}</strong>. Vui lòng nhập mã này để đặt lại mật khẩu."
                );
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi gửi email: {ex.Message}";
                return RedirectToPage("./ForgotPassword");
            }

            return RedirectToPage("VerifyOtp");
        }
    }
}