using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRsystem.Migrations
{
    /// <inheritdoc />
    public partial class UpdateEmployeeModell : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Employees",
                newName: "UserName");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_Username",
                table: "Employees",
                newName: "IX_Employees_UserName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserName",
                table: "Employees",
                newName: "Username");

            migrationBuilder.RenameIndex(
                name: "IX_Employees_UserName",
                table: "Employees",
                newName: "IX_Employees_Username");
        }
    }
}
