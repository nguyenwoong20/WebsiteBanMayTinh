#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Website_BanMayTinh.Models;

namespace Website_BanMayTinh.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Họ và tên là bắt buộc")]
            [StringLength(47, ErrorMessage = "Họ và tên phải có độ dài tối đa 47 ký tự.")]
            [RegularExpression(@"^\S[a-zA-ZÀ-ỹà-ỹĂăÂâĐđÊêÔôƠơƯư\s]*$", ErrorMessage = "Họ và tên không được bắt đầu bằng khoảng trắng và không được chứa ký tự đặc biệt.")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; }

            // / Tuổi có thể là số, bắt buộc phải là số nguyên dương, không được để trống và phải bé hơn 100. nếu có lỗi in ra thông báo lỗi
            [Required(ErrorMessage = "Tuổi là bắt buộc")]
            [Range(1, 100, ErrorMessage = "Tuổi phải từ 1 tuổi đến 100 tuổi")]
            [RegularExpression(@"^\d+$", ErrorMessage = "Tuổi phải là số dương")]
            [Display(Name = "Tuổi")]
            [StringLength(3, ErrorMessage = "Tuổi không được quá 3 ký tự.")]
            public string Age { get; set; }

            //Số điện thoại nhiều nhất là 11 ký tự, ít nhất là 10 ký tự. Thông báo lỗi nếu không đúng định dạng và phải là số
            [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
            [StringLength(11, ErrorMessage = "Số điện thoại phải có ít nhất 10 ký tự và nhiều nhất 11 ký tự.")]
            [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại phải là số và có ít nhất 10 ký tự và nhiều nhất 11 ký tự.")]
            [Display(Name = "Số điện thoại")]
            public string PhoneNumber { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Email là bắt buộc")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
            [StringLength(100, ErrorMessage = "{0} phải có độ dài từ {2} đến {1} ký tự.\"", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu không khớp.")]
            public string ConfirmPassword { get; set; }

            public string? Role { get; set; }
            [ValidateNever]
            public IEnumerable<SelectListItem> RoleList { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!_roleManager.RoleExistsAsync(SD.Role_Customer).GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Customer));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin));
                await _roleManager.CreateAsync(new IdentityRole(SD.Role_Company));
            }

            Input = new()
            {
                RoleList = _roleManager.Roles.Select(x => new SelectListItem
                {
                    Text = x.Name,
                    Value = x.Name
                })
            };

            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (ModelState.IsValid)
            {
                // 1. Tạo OTP
                var otp = new Random().Next(100000, 999999).ToString();

                // 2. Gửi email chứa OTP
                await _emailSender.SendEmailAsync(Input.Email, "Xác minh OTP đăng ký",
                    $"<p>Mã OTP của bạn là: <strong>{otp}</strong></p>");

                // 3. Lưu thông tin vào session để sử dụng khi xác minh
                HttpContext.Session.SetString("RegisterOtp_" + Input.Email, otp);
                HttpContext.Session.SetString("Register_FullName", Input.FullName);
                HttpContext.Session.SetString("Register_Age", Input.Age);
                HttpContext.Session.SetString("Register_Phone", Input.PhoneNumber);
                HttpContext.Session.SetString("Register_Password", Input.Password);
                HttpContext.Session.SetString("Register_Role", Input.Role ?? "");

                // 4. Chuyển sang trang xác minh OTP
                return RedirectToPage("VerifyOtpRegister", new { email = Input.Email });
            }

            // Nếu lỗi, hiển thị lại form
            return Page();
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Không thể khởi tạo lớp '{nameof(ApplicationUser)}'.");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("Identity mặc định yêu cầu UserStore hỗ trợ email.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}