using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Employee_management_system.Models.Entities;
using Employee_management_system.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Employee_Management_System.Data;

namespace Employee_management_system.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ManagerController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public ManagerController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAllManagers()
        {
            var allUsers = await _userManager.Users
                .Include(u => u.EmployeeProfile)
                .Include(u => u.Department)
                .ToListAsync();

            var managers = new List<ApplicationUser>();
            foreach (var user in allUsers)
            {
                if (await _userManager.IsInRoleAsync(user, "Manager"))
                {
                    managers.Add(user);
                }
            }

            var managerViewModels = managers.Select(u => new ManagerViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                DepartmentName = u.Department?.DepartmentName,
                IsActive = u.IsActive
            }).ToList();

            return Json(new { data = managerViewModels });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Departments = _context.Departments.ToList();
            return PartialView("_CreateManager", new ManagerViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ManagerViewModel model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Departments = _context.Departments.ToList();
                return PartialView("_CreateManager", model);
            }

            var department = await _context.Departments.FindAsync(model.DepartmentId);
            if (department == null)
            {
                ModelState.AddModelError("DepartmentId", "Selected department does not exist");
                ViewBag.Departments = _context.Departments.ToList();
                return PartialView("_CreateManager", model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                DepartmentId = model.DepartmentId,
                EmailConfirmed = true,
                IsActive = model.IsActive
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Manager");

                var employee = new Employee
                {
                    FirstName = model.FullName.Split(' ').First(),
                    LastName = model.FullName.Split(' ').Last(),
                    Phone = model.PhoneNumber,
                    DepartmentId = model.DepartmentId.Value,
                    IdentityUserId = user.Id,
                    IsActive = model.IsActive
                };
                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                user.EmployeeProfileId = employee.Id;
                _context.Update(user);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Manager created successfully!" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Departments = _context.Departments.ToList();
            return PartialView("_CreateManager", model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userManager.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            var model = new ManagerViewModel
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                DepartmentId = user.DepartmentId,
                IsActive = user.IsActive
            };

            ViewBag.Departments = _context.Departments.ToList();
            return PartialView("_EditManager", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ManagerViewModel model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Departments = _context.Departments.ToList();
                return PartialView("_EditManager", model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Manager not found");
                ViewBag.Departments = _context.Departments.ToList();
                return PartialView("_EditManager", model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.IsActive = model.IsActive;
            user.DepartmentId = model.DepartmentId;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return Json(new { success = true, message = "Manager updated successfully!" });
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Departments = _context.Departments.ToList();
            return PartialView("_EditManager", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return Json(new { success = false, message = "Invalid Manager ID." });
            }

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    return Json(new { success = false, message = "Manager not found." });
                }

                if (!await _userManager.IsInRoleAsync(user, "Manager"))
                {
                    return Json(new { success = false, message = "User is not a Manager." });
                }

                var employees = _context.Employees.Where(e => e.IdentityUserId == id).ToList();
                if (employees.Any())
                {
                    _context.Employees.RemoveRange(employees);
                    await _context.SaveChangesAsync();
                }

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Manager and associated employees deleted successfully." });
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return Json(new { success = false, message = $"Failed to delete manager: {errors}" });
                }
            }
            catch (DbUpdateException dbEx)
            {
                var baseMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                return Json(new
                {
                    success = false,
                    message = "Database error occurred: " + baseMessage
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Unexpected error: {ex.Message}"
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var isActiveFilter = Request.Form["isActive"].FirstOrDefault();

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var query = _context.Employees
                    .Include(e => e.IdentityUser)
                    .Include(e => e.Department)
                    .Where(e => e.DepartmentId == currentUser.DepartmentId)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchValue))
                {
                    query = query.Where(e =>
                        e.FirstName.Contains(searchValue) ||
                        e.LastName.Contains(searchValue) ||
                        e.IdentityUser.Email.Contains(searchValue) ||
                        e.Department.DepartmentName.Contains(searchValue));
                }

                if (!string.IsNullOrWhiteSpace(isActiveFilter))
                {
                    bool isActive = bool.Parse(isActiveFilter);
                    query = query.Where(e => e.IsActive == isActive);
                }

                var recordsTotal = await query.CountAsync();
                var data = await query.Skip(skip).Take(pageSize).ToListAsync();

                var result = data.Select(e => new
                {
                    e.Id,
                    e.FirstName,
                    e.LastName,
                    e.Phone,
                    Email = e.IdentityUser?.Email ?? "",
                    DepartmentName = e.Department?.DepartmentName ?? "",
                    IsActive = e.IsActive ? "Yes" : "No"
                });

                return Json(new
                {
                    draw,
                    recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Server error: " + ex.Message });
            }
        }

        public IActionResult Manage()
        {
            return View();
        }
    }
}