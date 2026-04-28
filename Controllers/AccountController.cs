using BuyZaar.Models;
using BuyZaar.Services;
using BuyZaar.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BuyZaar.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
        }

        // Register method (unchanged)
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

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

                var confirmationLink = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, token = token },
                    protocol: Request.Scheme
                );

                _emailService.SendEmail(
                    user.Email!,
                    "Verify your Buyzaar account",
                    $@"
                        <h2>Welcome to Buyzaar!</h2>
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

        // Email verification methods (unchanged)
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

            var result = await _userManager.ConfirmEmailAsync(user, token);

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

        // Login method with redirection based on role
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

    var isSuperAdmin = await _userManager.IsInRoleAsync(user, "SuperAdmin");
    var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

    // Require email verification only for non-admin accounts
    if (!isSuperAdmin && !isAdmin && !await _userManager.IsEmailConfirmedAsync(user))
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
    // Redirect to the appropriate dashboard based on the role
    if (isSuperAdmin)
        return RedirectToAction("Index", "SuperAdmin");

    if (isAdmin)
        return RedirectToAction("Index", "Admin");

    if (await _userManager.IsInRoleAsync(user, "Seller"))
        return RedirectToAction("Index", "Seller");  // Redirect to Seller Dashboard

    if (await _userManager.IsInRoleAsync(user, "Shopper"))
        return RedirectToAction("Index", "Shopper");  // Redirect to Shopper Dashboard

    return RedirectToAction("Index", "Home");  // Default redirect for other roles
}

    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
    return View(model);
}
       // Logout method
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Logout()
{
    await _signInManager.SignOutAsync();  // Sign out the user
    return RedirectToAction("Login", "Account");  // Redirect to the login page after logout
}
    }
}