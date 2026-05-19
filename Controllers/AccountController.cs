using System.Net;
using System.Text;
using BuyZaar.Data;
using BuyZaar.Models;
using BuyZaar.Services;
using BuyZaar.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace BuyZaar.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailService emailService,
            AppDbContext context,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
            _config = config;
        }

        private async Task<SystemSetting> GetSystemSettingsAsync()
        {
            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSetting
                {
                    AllowShopperRegistration = true,
                    AllowSellerRegistration = true,
                    AllowRiderRegistration = true,
                    UpdatedAt = DateTime.Now
                };

                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            return settings;
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            var settings = await GetSystemSettingsAsync();

            if (!settings.AllowShopperRegistration)
            {
                TempData["RegistrationDisabledMessage"] =
                    "Shopper registration is currently disabled by the SuperAdmin.";

                return RedirectToAction(nameof(Login));
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var settings = await GetSystemSettingsAsync();

            if (!settings.AllowShopperRegistration)
            {
                TempData["RegistrationDisabledMessage"] =
                    "Shopper registration is currently disabled by the SuperAdmin.";

                return RedirectToAction(nameof(Login));
            }

            if (!ModelState.IsValid)
                return View(model);

            var existingEmail = await _userManager.FindByEmailAsync(model.Email);

            if (existingEmail != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Email is already registered.");
                return View(model);
            }

            var existingUsername = await _userManager.FindByNameAsync(model.UserName);

            if (existingUsername != null)
            {
                ModelState.AddModelError(nameof(model.UserName), "Username is already taken.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                Email = model.Email,
                UserName = model.UserName,
                IsVerified = false,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Shopper");

                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var encodedToken = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(token)
                );

                var confirmationPath = Url.Action(
                    action: "ConfirmEmail",
                    controller: "Account",
                    values: new
                    {
                        userId = user.Id,
                        token = encodedToken
                    }
                );

                var confirmationLink = BuildFullUrl(confirmationPath);

                var emailBody = BuildEmailButtonTemplate(
                    title: "Welcome to BuyZaar!",
                    greeting: $"Hello {user.FullName},",
                    message: "Thank you for registering. Please verify your email using the link below.",
                    buttonText: "Verify My Account",
                    link: confirmationLink,
                    footer: "If you did not create this account, you may safely ignore this email."
                );

                _emailService.SendEmail(
                    user.Email!,
                    "Verify your BuyZaar account",
                    emailBody
                );

                return RedirectToAction("VerifyEmailNotice", new { email = user.Email });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmailNotice(string email)
        {
            ViewBag.Email = email;
            return View("~/Views/Email/VerifyEmailNotice.cshtml");
        }

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return Content("Invalid email confirmation link.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return Content("User not found.");

            string decodedToken;

            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                return Content("Invalid email confirmation token.");
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                user.IsVerified = true;
                user.VerifiedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                return RedirectToAction("EmailVerifiedSuccess");
            }

            return Content("Email confirmation failed.");
        }

        [HttpGet]
        public IActionResult EmailVerifiedSuccess()
        {
            return View("~/Views/Email/EmailVerifiedSuccess.cshtml");
        }

        [HttpGet]
        public IActionResult SetupPassword(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return Content("Invalid password setup link.");

            var model = new SetupPasswordViewModel
            {
                UserId = userId,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetupPassword(SetupPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user == null)
                return Content("User not found.");

            var result = await _userManager.ResetPasswordAsync(
                user,
                model.Token,
                model.Password
            );

            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                user.IsVerified = true;
                user.VerifiedAt = DateTime.UtcNow;

                await _userManager.UpdateAsync(user);

                var riderProfile = await _context.RiderProfiles
                    .FirstOrDefaultAsync(r => r.UserId == user.Id);

                if (riderProfile != null)
                {
                    riderProfile.Status = "Active";
                    await _context.SaveChangesAsync();
                }

                TempData["LoginMessage"] = "Password setup successful. You can now log in.";
                return RedirectToAction("Login", "Account");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }

            if (IsUserDeactivated(user))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Your account has been deactivated by the administrator."
                );

                return View(model);
            }

            var isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isRider = await _userManager.IsInRoleAsync(user, "Rider");

            if (!isSuperAdmin &&
                !isAdmin &&
                !isRider &&
                !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, "Please verify your email before logging in.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                if (isSuperAdmin)
                    return RedirectToAction("Index", "SuperAdmin");

                if (isAdmin)
                    return RedirectToAction("Index", "Admin");

                if (isRider)
                    return RedirectToAction("Index", "Rider");

                if (await _userManager.IsInRoleAsync(user, "Seller"))
                    return RedirectToAction("Index", "Seller");

                if (await _userManager.IsInRoleAsync(user, "Shopper"))
                    return RedirectToAction("BrowseProducts", "Shopper");

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["ForgotPasswordMessage"] =
                    "If the email exists and is verified, a password reset link has been sent.";

                return RedirectToAction(nameof(ForgotPassword));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token)
            );

            var resetPath = Url.Action(
                action: "ResetPassword",
                controller: "Account",
                values: new
                {
                    email = user.Email,
                    token = encodedToken
                }
            );

            var resetLink = BuildFullUrl(resetPath);

            var resetEmailBody = BuildEmailButtonTemplate(
                title: "Password Reset Request",
                greeting: $"Hello {user.FullName},",
                message: "You requested to reset your BuyZaar password. Use the link below to create a new password.",
                buttonText: "Reset My Password",
                link: resetLink,
                footer: "If you did not request this, you can safely ignore this email."
            );

            _emailService.SendEmail(
                user.Email!,
                "Reset your BuyZaar password",
                resetEmailBody
            );

            TempData["ForgotPasswordMessage"] =
                "If the email exists and is verified, a password reset link has been sent.";

            return RedirectToAction(nameof(ForgotPassword));
        }

        [HttpGet]
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
                return RedirectToAction(nameof(Login));

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                TempData["LoginMessage"] = "Password reset successful. You can now log in.";
                return RedirectToAction(nameof(Login));
            }

            string decodedToken;

            try
            {
                var tokenBytes = WebEncoders.Base64UrlDecode(model.Token);
                decodedToken = Encoding.UTF8.GetString(tokenBytes);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Invalid password reset token.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(
                user,
                decodedToken,
                model.Password
            );

            if (result.Succeeded)
            {
                TempData["LoginMessage"] = "Password reset successful. You can now log in.";
                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }

        private string BuildFullUrl(string? path)
        {
            var baseUrl = _config["AppSettings:BaseUrl"];

            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                baseUrl = $"https://{Request.Host.Value}";
            }

            baseUrl = baseUrl.TrimEnd('/');

            if (string.IsNullOrWhiteSpace(path))
            {
                return baseUrl;
            }

            return $"{baseUrl}{path}";
        }

        private string BuildEmailButtonTemplate(
            string title,
            string greeting,
            string message,
            string buttonText,
            string link,
            string footer)
        {
            var safeTitle = WebUtility.HtmlEncode(title);
            var safeGreeting = WebUtility.HtmlEncode(greeting);
            var safeMessage = WebUtility.HtmlEncode(message);
            var safeButtonText = WebUtility.HtmlEncode(buttonText);
            var safeLink = WebUtility.HtmlEncode(link);
            var safeFooter = WebUtility.HtmlEncode(footer);

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>{safeTitle}</title>
</head>
<body style=""margin:0; padding:0; background-color:#ffffff; font-family:Arial, Helvetica, sans-serif;"">

    <div style=""max-width:600px; margin:0 auto; padding:24px;"">

        <h2 style=""color:#111827; font-size:24px; line-height:1.3; margin-bottom:16px;"">
            {safeTitle}
        </h2>

        <p style=""color:#374151; font-size:16px; line-height:1.6;"">
            {safeGreeting}
        </p>

        <p style=""color:#374151; font-size:16px; line-height:1.6;"">
            {safeMessage}
        </p>

        <p style=""margin:28px 0;"">
            <a href=""{safeLink}""
               target=""_blank""
               style=""color:#ffffff;
                      background-color:#2563eb;
                      padding:14px 22px;
                      text-decoration:none;
                      border-radius:6px;
                      display:inline-block;
                      font-weight:bold;"">
                {safeButtonText}
            </a>
        </p>

        <p style=""color:#374151; font-size:15px; line-height:1.6;"">
            Or open this verification link:
        </p>

        <p style=""font-size:15px; line-height:1.6; word-break:break-all;"">
            <a href=""{safeLink}""
               target=""_blank""
               style=""color:#2563eb; text-decoration:underline;"">
                {safeLink}
            </a>
        </p>

        <p style=""color:#6b7280; font-size:14px; line-height:1.6; margin-top:24px;"">
            {safeFooter}
        </p>

    </div>

</body>
</html>";
        }

        private bool IsUserDeactivated(ApplicationUser user)
        {
            return user.LockoutEnd != null &&
                   user.LockoutEnd > DateTimeOffset.UtcNow;
        }
    }
}