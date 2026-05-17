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

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailService emailService,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _context = context;
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

                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new
                    {
                        userId = user.Id,
                        token = encodedToken
                    },
                    protocol: Request.Scheme
                );

                _emailService.SendEmail(
                    user.Email!,
                    "Verify your BuyZaar account",
                    $@"
                        <h2>Welcome to BuyZaar!</h2>
                        <p>Thank you for registering.</p>
                        <p>Please verify your email by clicking the link below:</p>
                        <p><a href='{confirmationLink}'>Verify My Account</a></p>
                        <p>If you did not create this account, you may ignore this email.</p>
                    "
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

            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new
                {
                    email = user.Email,
                    token = encodedToken
                },
                protocol: Request.Scheme
            );

            _emailService.SendEmail(
                user.Email!,
                "Reset your BuyZaar password",
                $@"
                    <h2>Password Reset Request</h2>
                    <p>Hello {user.FullName},</p>
                    <p>You requested to reset your BuyZaar password.</p>
                    <p>Click the link below to create a new password:</p>
                    <p><a href='{resetLink}'>Reset My Password</a></p>
                    <p>If you did not request this, you can safely ignore this email.</p>
                "
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

        private bool IsUserDeactivated(ApplicationUser user)
        {
            return user.LockoutEnd != null &&
                   user.LockoutEnd > DateTimeOffset.UtcNow;
        }
    }
}