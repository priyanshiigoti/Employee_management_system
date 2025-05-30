﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Employee_Management_System.Data;
using Employee_management_system.Models.Entities;
using Employee_management_system.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Employee_management_system.Services;

namespace Employee_Management_System.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var jwtUser = JwtHelper.GetJwtUser(HttpContext);

            if (!jwtUser.IsAuthenticated || string.IsNullOrEmpty(jwtUser.Email))
            {
                return Unauthorized("User not authenticated or email missing.");
            }

            var currentUser = await _userManager.FindByEmailAsync(jwtUser.Email);
            if (currentUser == null)
            {
                return Unauthorized("User not found in database.");
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);
            if (userRoles == null)
            {
                return BadRequest("User roles not found.");
            }

            var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
            var managerUserIds = managerUsers.Select(u => u.Id).ToList();

            var query = _context.Employees
                .Include(e => e.Department)
                .Include(e => e.IdentityUser)
                .Where(e => !managerUserIds.Contains(e.IdentityUserId));

            if (userRoles.Contains("Manager") && currentUser.DepartmentId != null)
            {
                query = query.Where(e => e.DepartmentId == currentUser.DepartmentId);
            }

            var employees = await query.ToListAsync();

            var viewModels = employees.Select(e => new EmployeeViewModel
            {
                Id = e.Id,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Phone = e.Phone,
                Email = e.IdentityUser?.Email ?? "",
                IsActive = e.IsActive,
                DepartmentName = e.Department?.DepartmentName ?? ""
            }).ToList();

            return View(viewModels);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var jwtUser = JwtHelper.GetJwtUser(HttpContext);
                if (!jwtUser.IsAuthenticated || string.IsNullOrEmpty(jwtUser.Email))
                {
                    return Json(new { error = "User not authenticated." });
                }

                var currentUser = await _userManager.FindByEmailAsync(jwtUser.Email);
                if (currentUser == null)
                {
                    return Json(new { error = "User not found." });
                }

                var userRoles = await _userManager.GetRolesAsync(currentUser);
                if (userRoles == null)
                {
                    return Json(new { error = "User roles not found." });
                }

                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                var isActiveFilter = Request.Form["isActive"].FirstOrDefault();

                // Read sorting info
                var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
                var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // asc or desc

                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;

                var managerUsers = await _userManager.GetUsersInRoleAsync("Manager");
                var managerUserIds = managerUsers.Select(u => u.Id).ToList();

                var query = _context.Employees
                    .Include(e => e.IdentityUser)
                    .Include(e => e.Department)
                    .Where(e => !managerUserIds.Contains(e.IdentityUserId))
                    .AsQueryable();

                if (userRoles.Contains("Manager"))
                {
                    query = query.Where(e => e.DepartmentId == currentUser.DepartmentId);
                }

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

                // Apply sorting
                // Map the column index from DataTables to entity properties
                switch (sortColumnIndex)
                {
                    case "0": // Assuming column 0 = FirstName
                        query = (sortDirection == "asc") ? query.OrderBy(e => e.FirstName) : query.OrderByDescending(e => e.FirstName);
                        break;
                    case "1": // LastName
                        query = (sortDirection == "asc") ? query.OrderBy(e => e.LastName) : query.OrderByDescending(e => e.LastName);
                        break;
                    case "2": // Phone
                        query = (sortDirection == "asc") ? query.OrderBy(e => e.Phone) : query.OrderByDescending(e => e.Phone);
                        break;
                    case "3": // Email
                        query = (sortDirection == "asc") ? query.OrderBy(e => e.IdentityUser.Email) : query.OrderByDescending(e => e.IdentityUser.Email);
                        break;
                    case "4": // DepartmentName
                        query = (sortDirection == "asc") ? query.OrderBy(e => e.Department.DepartmentName) : query.OrderByDescending(e => e.Department.DepartmentName);
                        break;
                    case "5": // IsActive
                        query = (sortDirection == "asc") ? query.OrderBy(e => e.IsActive) : query.OrderByDescending(e => e.IsActive);
                        break;
                    default:
                        // Default sorting by Id
                        query = query.OrderBy(e => e.Id);
                        break;
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
                    IsActive = e.IsActive ? "Yes" : "No",
                    Actions = $"<button class='btn btn-sm btn-primary edit' data-id='{e.Id}'>Edit</button> " +
                              $"<button class='btn btn-sm btn-danger delete' data-id='{e.Id}'>Delete</button>"
                });

                return Json(new
                {
                    draw = draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsTotal,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return Json(new { error = "Server error: " + ex.Message });
            }
        }


        public IActionResult Create()
        {
            var vm = new EmployeeViewModel
            {
                Departments = new SelectList(_context.Departments, "Id", "DepartmentName")
            };
            return PartialView("_CreateOrEdit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Departments = new SelectList(_context.Departments, "Id", "DepartmentName", vm.DepartmentId);
                return PartialView("_CreateOrEdit", vm);
            }

            var existingUser = await _userManager.FindByEmailAsync(vm.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "An account with this email already exists.");
                vm.Departments = new SelectList(_context.Departments, "Id", "DepartmentName", vm.DepartmentId);
                return PartialView("_CreateOrEdit", vm);
            }

            bool phoneExists = await _context.Employees.AnyAsync(e => e.Phone == vm.Phone);
            if (!string.IsNullOrWhiteSpace(vm.Phone) && phoneExists)
            {
                ModelState.AddModelError("Phone", "An employee with this phone number already exists.");
                vm.Departments = new SelectList(_context.Departments, "Id", "DepartmentName", vm.DepartmentId);
                return PartialView("_CreateOrEdit", vm);
            }

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                EmailConfirmed = true,
                PhoneNumber = vm.Phone,
                DepartmentId = vm.DepartmentId
            };

            var result = await _userManager.CreateAsync(user, "Default@123");
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                vm.Departments = new SelectList(_context.Departments, "Id", "DepartmentName", vm.DepartmentId);
                return PartialView("_CreateOrEdit", vm);
            }

            await _userManager.AddToRoleAsync(user, "Employee");

            var employee = new Employee
            {
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Phone = vm.Phone,
                DepartmentId = vm.DepartmentId,
                IdentityUserId = user.Id,
                IsActive = vm.IsActive
            };

            _context.Add(employee);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }


        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.IdentityUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound();

            var vm = new EmployeeViewModel
            {
                Id = employee.Id,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Phone = employee.Phone,
                Email = employee.IdentityUser?.Email ?? "",
                IsActive = employee.IsActive,
                DepartmentId = employee.DepartmentId,
                Departments = new SelectList(_context.Departments, "Id", "DepartmentName", employee.DepartmentId)
            };

            return PartialView("_CreateOrEdit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel vm)
        {
            if (id != vm.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                vm.Departments = new SelectList(_context.Departments, "Id", "DepartmentName", vm.DepartmentId);
                return PartialView("_CreateOrEdit", vm);
            }

            var employee = await _context.Employees
                .Include(e => e.IdentityUser)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
                return NotFound();

            if (employee.IdentityUser?.Email != vm.Email)
            {
                employee.IdentityUser.Email = vm.Email;
                employee.IdentityUser.UserName = vm.Email;
                await _userManager.UpdateAsync(employee.IdentityUser);
            }

            employee.FirstName = vm.FirstName;
            employee.LastName = vm.LastName;
            employee.Phone = vm.Phone;
            employee.DepartmentId = vm.DepartmentId;
            employee.IsActive = vm.IsActive;

            _context.Update(employee);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return Json(new { success = false, message = "Not found" });

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

    }
}
