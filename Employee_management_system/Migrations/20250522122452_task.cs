using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee_management_system.Migrations
{
    /// <inheritdoc />
    public partial class task : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "ce50a0e2-a6e0-4ac5-8e2d-14e5b0a160a3", "AQAAAAIAAYagAAAAEAtLiLwKLIQe2z3JAVJzCkeb/ESG+6VTzqvn7hloPV88MImyvGtEMV+ffI5aJF+DSw==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "bb4dc779-65d5-4f21-9136-e9c5595c55c5", "AQAAAAIAAYagAAAAENqxaIIv4tAWa6BjqhDkShorCM5+fb9cHbuaXlBLROx7gWgiUZ/BsXRjnKLViuRPGw==" });
        }
    }
}
