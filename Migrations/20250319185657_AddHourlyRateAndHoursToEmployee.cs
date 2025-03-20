using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRsystem.Migrations
{
    /// <inheritdoc />
    public partial class AddHourlyRateAndHoursToEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyRate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkEndTime",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "WorkStartTime",
                table: "Employees");

            migrationBuilder.CreateTable(
                name: "WorkSchedules",
                columns: table => new
                {
                    WorkScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    HourlyRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    WorkStartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    WorkEndTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkSchedules", x => x.WorkScheduleId);
                    table.ForeignKey(
                        name: "FK_WorkSchedules_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkSchedules_EmployeeId",
                table: "WorkSchedules",
                column: "EmployeeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WorkSchedules");

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyRate",
                table: "Employees",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

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
    }
}
