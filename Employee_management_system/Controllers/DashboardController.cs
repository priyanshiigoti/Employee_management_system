using Employee_management_system.Models;
using Employee_management_system.Models.Entities;
using Employee_management_system.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Employee_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            var token = Request.Cookies["JwtToken"]; // Get token from cookie only
            ViewBag.JwtToken = token;
            return View();
        }


        [Authorize(Roles = "Manager")]
        public IActionResult ManagerDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> EmployeeDashboard()
        {
            // Check JWT authentication
            var jwtUser = JwtHelper.GetJwtUser(HttpContext);
            if (jwtUser == null || !jwtUser.IsAuthenticated || string.IsNullOrEmpty(jwtUser.Email) || string.IsNullOrEmpty(jwtUser.UserId))
            {
                return Json(new { error = "User not authenticated." });
            }

            // Use UserId from JWT, NOT from ClaimsPrincipal (User)
            var userId = jwtUser.UserId;

            var user = await _userManager.Users
                .Include(u => u.Department)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var model = new EmployeeViewModel
            {
                FirstName = user.FullName, // Maps FullName to FirstName
                Email = user.Email,
                Phone = user.PhoneNumber,
                DepartmentName = user.Department?.DepartmentName ?? "N/A"
            };

            return View(model);
        }


    }
}
