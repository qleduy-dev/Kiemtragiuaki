using System.ComponentModel.DataAnnotations;
using Kiemtragiuaki.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiemtragiuaki.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;
        private readonly UserManager<AspNetUser> _userManager;

        public LoginModel(SignInManager<AspNetUser> signInManager, ILogger<LoginModel> logger, UserManager<AspNetUser> userManager)
        {
            _signInManager = signInManager;
            _logger = logger;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public IList<AuthenticationScheme> ExternalLogins { get; set; } = new List<AuthenticationScheme>();

        [TempData]
        public string? ErrorMessage { get; set; }

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return LocalRedirect(GetSafeReturnUrl(returnUrl, Url.Content("~/")));

            if (!string.IsNullOrEmpty(ErrorMessage))
                ModelState.AddModelError(string.Empty, ErrorMessage);

            ReturnUrl = GetSafeReturnUrl(returnUrl, Url.Content("~/"));

            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = GetSafeReturnUrl(returnUrl, Url.Content("~/Enrollments/MyCourses"));
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            if (!ModelState.IsValid)
                return Page();

            var email = Input.Email.Trim();
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(user, Input.Password, Input.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User logged in.");

                if (await _userManager.IsInRoleAsync(user, "ADMIN"))
                    return LocalRedirect(Url.Content("~/Courses"));

                return LocalRedirect(ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đang bị khóa tạm thời do đăng nhập sai quá nhiều lần.");
                return Page();
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return Page();
        }

        private string GetSafeReturnUrl(string? returnUrl, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return returnUrl;

            return fallback;
        }
    }
}
