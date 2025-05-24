using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Employee_Management_System.Models;
using Employee_management_system.Models.Entities; // Adjust namespace to match your project

namespace Employee_Management_System.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<ManagerDepartment> ManagerDepartments { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Define relationships
            builder.Entity<Employee>()
                .HasOne(e => e.IdentityUser)
                .WithMany()
                .HasForeignKey(e => e.IdentityUserId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents cascade delete


            builder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId);

            builder.Entity<ManagerDepartment>()
                .HasOne(md => md.IdentityUser)
                .WithMany()
                .HasForeignKey(md => md.IdentityUserId);

            builder.Entity<ManagerDepartment>()
                .HasOne(md => md.Department)
                .WithMany()
                .HasForeignKey(md => md.DepartmentId);

            builder.Entity<ApplicationUser>()
     .HasOne(a => a.Department)
     .WithMany()
     .HasForeignKey(a => a.DepartmentId)
     .OnDelete(DeleteBehavior.Restrict);

            // Seed Roles
            var adminRole = new IdentityRole
            {
                Id = "1",
                Name = "Admin",
                NormalizedName = "ADMIN"
            };

            var managerRole = new IdentityRole
            {
                Id = "2",
                Name = "Manager",
                NormalizedName = "MANAGER"
            };

            var employeeRole = new IdentityRole
            {
                Id = "3",
                Name = "Employee",
                NormalizedName = "EMPLOYEE"
            };

            builder.Entity<IdentityRole>().HasData(adminRole, managerRole, employeeRole);

            // Seed Admin User
            var hasher = new PasswordHasher<ApplicationUser>();
            var adminUser = new ApplicationUser
            {
                Id = "1",
                UserName = "admin@admin.com",
                NormalizedUserName = "ADMIN@ADMIN.COM",
                Email = "admin@admin.com",
                NormalizedEmail = "ADMIN@ADMIN.COM",
                EmailConfirmed = true,
                SecurityStamp = "STATIC-SECURITY-STAMP", // Hardcoded string
                FullName = "System Administrator" ,// Assuming you have FullName property in ApplicationUser
                    CreatedAt = new DateTime(2024, 01, 01),    //  Static date

            };

            adminUser.PasswordHash = hasher.HashPassword(adminUser, "Admin@123");

            builder.Entity<ApplicationUser>().HasData(adminUser);

            // Assign Admin Role to Admin User
            builder.Entity<IdentityUserRole<string>>().HasData(new IdentityUserRole<string>
            {
                UserId = "1",
                RoleId = "1"
            });
        }
    }
}
