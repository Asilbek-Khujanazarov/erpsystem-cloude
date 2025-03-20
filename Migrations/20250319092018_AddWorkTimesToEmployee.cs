using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkTimesToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiredMonthlyHours",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Salary",
                table: "Employees");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkEndTime",
                table: "Employees",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "WorkStartTime",
                table: "Employees",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkEndTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkStartTime",
                table: "Employees");

            migrationBuilder.AddColumn<double>(
                name: "RequiredMonthlyHours",
                table: "Employees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "Salary",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
