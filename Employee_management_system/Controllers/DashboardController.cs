using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Employee_Management_System.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Manager")]
        public IActionResult ManagerDashboard()
        {
            return View();
        }

        [Authorize(Roles = "Employee")]
        public IActionResult EmployeeDashboard()
        {
            return View();
        }

    }
}
