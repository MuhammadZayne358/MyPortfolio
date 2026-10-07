using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyPortfolio.Models;

namespace MyPortfolio.Controllers
{
    public class IdentityController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;
        private readonly ILogger<IdentityController> _logger;

        public IdentityController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            EmailService emailService,
            ILogger<IdentityController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _logger = logger;
        }

        // ───────────── Sign up ─────────────

        [HttpGet]
        public IActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Signup(RegisterViewmodel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                if (error.Code == "DuplicateUserName" || error.Code == "DuplicateEmail")
                {
                    ModelState.AddModelError("Email", "This email is already taken.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // ───────────── Login / Logout ─────────────

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return LocalRedirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        // ───────────── Forgot / reset password ─────────────

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError(string.Empty, "Please enter your email address.");
                return View();
            }

            var user = await _userManager.FindByEmailAsync(email.Trim());
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No account found with this email address.");
                return View();
            }
            if (user != null)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resetLink = Url.Action("ResetPassword", "Identity",
                    new { token, email = user.Email }, Request.Scheme);

                try
                {
                    await _emailService.SendEmailAsync(user.Email!, "Password Reset Request",
                        "<p>Reset your password by clicking the link below:</p>" +
                        $"<a href='{resetLink}'>Reset Password</a>");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not send the password reset email.");
                    ModelState.AddModelError(string.Empty, "We couldn't send the email right now. Please try again later.");
                    return View();
                }
            }

            // Same message whether or not the account exists, so emails can't be probed.
            ViewData["EmailSent"] = true;
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            var model = new ResetPassword
            {
                Email = email,
                Token = token
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "This reset link is not valid. Please request a new one.");
                return View(model);
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (result.Succeeded)
            {
                TempData["AuthMessage"] = "Your password has been reset. You can sign in now.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
    }
}
