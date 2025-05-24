using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Employee_management_system.Migrations
{
    /// <inheritdoc />
    public partial class active : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskAssignments");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "IsActive", "PasswordHash" },
                values: new object[] { "4104ff23-fba4-4a2d-819c-9b10cb1de15c", false, "AQAAAAIAAYagAAAAEOr0i5nSqgYshrEjPu61V9zgltfrLiFDUG2KAD5SRUnRfatE+sp1EP52Jmqc6lLIQQ==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "TaskAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaskAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "206899af-e7f2-4127-8d83-7ba1be96f8ac", "AQAAAAIAAYagAAAAEB4eTDH1T1+5UDrLq5fGvmwE39edMPeOSHwAN2ETqAy/IOH7t8W2LhjQy28TJhruFQ==" });

            migrationBuilder.CreateIndex(
                name: "IX_TaskAssignments_EmployeeId",
                table: "TaskAssignments",
                column: "EmployeeId");
        }
    }
}
