using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Employee_Management_System.Data;
using Employee_management_system.Models.Entities;
using System.Linq.Dynamic.Core;
using Employee_management_system.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Employee_Management_System.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DepartmentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DepartmentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult GetAll()
        //{
        //    var draw = Request.Form["draw"].FirstOrDefault();
        //    var start = Request.Form["start"].FirstOrDefault();
        //    var length = Request.Form["length"].FirstOrDefault();
        //    var searchValue = Request.Form["search[value]"].FirstOrDefault();

        //    int pageSize = string.IsNullOrEmpty(length) ? 10 : int.Parse(length);
        //    int skip = string.IsNullOrEmpty(start) ? 0 : int.Parse(start);

        //    var query = _context.Departments.AsQueryable();

        //    if (!string.IsNullOrEmpty(searchValue))
        //    {
        //        query = query.Where(x => x.DepartmentName.Contains(searchValue));
        //    }

        //    int recordsTotal = query.Count();
        //    var data = query.Skip(skip).Take(pageSize).ToList();

        //    return Json(new
        //    {
        //        draw,
        //        recordsFiltered = recordsTotal,
        //        recordsTotal,
        //        data
        //    });
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GetAll()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Request.Form["start"].FirstOrDefault();
            var length = Request.Form["length"].FirstOrDefault();
            var searchValue = Request.Form["search[value]"].FirstOrDefault();

            // Sorting
            var sortColumnIndex = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // asc or desc

            int pageSize = string.IsNullOrEmpty(length) ? 10 : int.Parse(length);
            int skip = string.IsNullOrEmpty(start) ? 0 : int.Parse(start);

            var query = _context.Departments.AsQueryable();

            // Searching
            if (!string.IsNullOrEmpty(searchValue))
            {
                query = query.Where(x => x.DepartmentName.Contains(searchValue));
            }

            int recordsTotal = query.Count();

            // Define sortable columns in order matching your DataTable columns
            var columns = new[] { "DepartmentName" };

            if (!string.IsNullOrEmpty(sortColumnIndex) && int.TryParse(sortColumnIndex, out int sortCol))
            {
                string sortColumn = columns.ElementAtOrDefault(sortCol);
                if (!string.IsNullOrEmpty(sortColumn))
                {
                    query = query.OrderBy($"{sortColumn} {sortDirection}");
                }
            }
            else
            {
                // Default sorting if no sort info provided
                query = query.OrderBy(d => d.DepartmentName);
            }

            var data = query.Skip(skip).Take(pageSize).ToList();

            return Json(new
            {
                draw,
                recordsFiltered = recordsTotal,
                recordsTotal,
                data
            });
        }


        [HttpGet]
        public IActionResult Create()
        {
            var model = new DepartmentViewModel();
            return PartialView("_CreateOrEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DepartmentViewModel model)
        {
            if (!ModelState.IsValid)
                return PartialView("_CreateOrEdit", model);

            var exists = await _context.Departments
                .AnyAsync(d => d.DepartmentName.Trim().ToLower() == model.DepartmentName.Trim().ToLower());

            if (exists)
            {
                ModelState.AddModelError("DepartmentName", "A department with this name already exists.");
                return PartialView("_CreateOrEdit", model);
            }

            var department = new Department
            {
                DepartmentName = model.DepartmentName.Trim()
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Department created successfully." });
        }



        [HttpGet]
        public IActionResult Edit(int id)
        {
            var dept = _context.Departments.Find(id);
            if (dept == null) return NotFound();

            var model = new DepartmentViewModel
            {
                Id = dept.Id,
                DepartmentName = dept.DepartmentName,
            };

            return PartialView("_CreateOrEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(DepartmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                bool isDuplicate = _context.Departments
                    .Any(d => d.DepartmentName == model.DepartmentName && d.Id != model.Id);

                if (isDuplicate)
                {
                    ModelState.AddModelError("DepartmentName", "Department name already exists.");
                    return PartialView("_CreateOrEdit", model);
                }

                var dept = _context.Departments.Find(model.Id);
                if (dept == null) return NotFound();

                dept.DepartmentName = model.DepartmentName;
                _context.SaveChanges();

                return Json(new { success = true, message = "Department updated successfully." });
            }

            return PartialView("_CreateOrEdit", model);
        }



        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return Json(new { success = false, message = "Department not found." });

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Department deleted successfully." });
        }

    }
}
