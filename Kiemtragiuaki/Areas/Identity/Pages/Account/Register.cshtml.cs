using System.ComponentModel.DataAnnotations;
using Kiemtragiuaki.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Kiemtragiuaki.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<AspNetUser> _signInManager;
        private readonly UserManager<AspNetUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RegisterModel(
            UserManager<AspNetUser> userManager,
            SignInManager<AspNetUser> signInManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
            [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ {2} đến {1} ký tự")]
            [Display(Name = "Họ và tên")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            [StringLength(100, ErrorMessage = "Mật khẩu phải từ {2} đến {1} ký tự", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public IActionResult OnGet(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return LocalRedirect(Url.Content("~/"));

            ReturnUrl = GetSafeReturnUrl(returnUrl, Url.Content("~/Enrollments/MyCourses"));
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = GetSafeReturnUrl(returnUrl, Url.Content("~/Enrollments/MyCourses"));

            if (!ModelState.IsValid)
                return Page();

            var email = Input.Email.Trim();
            var existedUser = await _userManager.FindByEmailAsync(email);
            if (existedUser != null)
            {
                ModelState.AddModelError(nameof(Input.Email), "Email này đã được sử dụng.");
                return Page();
            }

            if (!await _roleManager.RoleExistsAsync("STUDENT"))
                await _roleManager.CreateAsync(new IdentityRole("STUDENT"));

            var user = new AspNetUser
            {
                UserName = email,
                Email = email,
                FullName = Input.FullName.Trim(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, Input.Password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return Page();
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "STUDENT");
            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);
                foreach (var error in roleResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return Page();
            }

            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công.";
            return LocalRedirect(ReturnUrl);
        }

        private string GetSafeReturnUrl(string? returnUrl, string fallback)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return returnUrl;

            return fallback;
        }
    }
}
