using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Encodings.Web;
using Employee_management_system.Models;
using Employee_management_system.Models.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Employee_Management_System.Data;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Employee_management_system.Services;

namespace employee_management.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtSettings;
        private readonly IConfiguration _configuration;


        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            ApplicationDbContext context,
            IOptions<JwtService> jwtOptions,
           IConfiguration configuration)

        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _context = context;
            _jwtSettings = jwtOptions.Value;
            _configuration = configuration;
        }

        public async Task<IActionResult> RegisterAsync()
        {
            ViewBag.Departments = await _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.DepartmentName
                }).ToListAsync();

            return View();
        }

        [HttpPost]

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = $"{model.FirstName} {model.LastName}",
                    PhoneNumber = model.Phone,
                    Gender = model.Gender,
                    DepartmentId = model.DepartmentId
                };


                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Employee");

                    var employee = new Employee
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Phone = model.Phone,
                        IsActive = true,
                        DepartmentId = model.DepartmentId,
                        IdentityUserId = user.Id
                    };

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    var callbackUrl = Url.Action("ConfirmEmail", "Account",
                        new { userId = user.Id, token }, Request.Scheme);

                    await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                        $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    return RedirectToAction("RegisterConfirmation", new { email = model.Email });
                }

                
                    var passwordErrors = result.Errors
                        .Where(e => e.Code.Contains("Password"))
                        .Select(e => e.Description)
                        .ToList();

                    foreach (var key in ModelState.Keys.Where(k => k.Contains("Password")).ToList())
                    {
                        ModelState[key].Errors.Clear();
                    }

                    foreach (var err in passwordErrors)
                    {
                        ModelState.AddModelError(string.Empty, err);
                    }

                    foreach (var error in result.Errors.Where(e => !e.Code.Contains("Password")))
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }

            

            ViewBag.Departments = await _context.Departments
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.DepartmentName
                }).ToListAsync();


            
        
            return View(model);
        }


        public IActionResult RegisterConfirmation(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            {
                TempData["ErrorMessage"] = "Invalid login attempt.";
                return View(model);
            }

            if (!user.EmailConfirmed)
            {
                TempData["ErrorMessage"] = "Please confirm your email first.";
                return View(model);
            }

            var roles = await _userManager.GetRolesAsync(user);

            // Create JWT token
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Save token in cookie or session (optional)
            Response.Cookies.Append("JwtToken", tokenString, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
            if (roles.Contains("Admin"))
                return RedirectToAction("AdminDashboard", "Dashboard");

            if (roles.Contains("Manager"))
                return RedirectToAction("ManagerDashboard", "Dashboard");

            if (roles.Contains("Employee"))
                return RedirectToAction("EmployeeDashboard", "Dashboard");

            return RedirectToAction("Index", "Home");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Sign out user
            await _signInManager.SignOutAsync();
            Response.Cookies.Delete("JwtToken");

            return RedirectToAction("Login", "Account");
        }


        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "No user associated with this email address.");
                return View(model);
            }

            if (user.EmailConfirmed == false)
            {
                TempData["ErrorMessage"] = "You need to confirm your email first.";
                return View(model);
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { token, email = model.Email }, Request.Scheme);

            await _emailSender.SendEmailAsync(model.Email, "Reset Password",
                $"Reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        public IActionResult ForgotPasswordConfirmation() => View();

        public IActionResult ResetPassword(string token, string email) => View(new ResetPasswordViewModel { Token = token, Email = email });

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        public IActionResult ResetPasswordConfirmation() => View();


        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var jwtUser = JwtHelper.GetJwtUser(HttpContext);
            if (!jwtUser.IsAuthenticated || string.IsNullOrEmpty(jwtUser.Email))
            {
                return Json(new { error = "User not authenticated." });
            }

            var user = await _userManager.FindByEmailAsync(jwtUser.Email);
            if (user == null) return NotFound();

            var model = new UpdateProfileViewModel
            {
                FullName = user.FullName,
                Gender = user.Gender,
                Email = user.Email
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(UpdateProfileViewModel model)
        {
            var jwtUser = JwtHelper.GetJwtUser(HttpContext);
            if (!jwtUser.IsAuthenticated || string.IsNullOrEmpty(jwtUser.Email))
            {
                return Json(new { error = "User not authenticated." });
            }

            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.FindByEmailAsync(jwtUser.Email);
            if (user == null) return NotFound();

            user.FullName = model.FullName;
            user.Gender = model.Gender;
            user.Email = model.Email;
            user.UserName = model.Email;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }

            TempData["Error"] = "Failed to update profile.";
            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var jwtUser = JwtHelper.GetJwtUser(HttpContext);
            if (!jwtUser.IsAuthenticated || string.IsNullOrEmpty(jwtUser.Email))
            {
                return Json(new { error = "User not authenticated." });
            }

            return View(new ChangePasswordViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            var jwtUser = JwtHelper.GetJwtUser(HttpContext);
            if (!jwtUser.IsAuthenticated || string.IsNullOrEmpty(jwtUser.Email))
            {
                return Json(new { error = "User not authenticated." });
            }

            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(jwtUser.Email);
            if (user == null)
                return RedirectToAction("Login");

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Password changed successfully!";
                return View();
            }

            TempData["ErrorMessage"] = "Password change failed.";
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(model);
        }


        public IActionResult AccessDenied()
        {

            return View();
        }

    }
}
