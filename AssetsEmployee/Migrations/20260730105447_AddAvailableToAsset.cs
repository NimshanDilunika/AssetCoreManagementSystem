using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AssetsEmployee.Migrations
{
    /// <inheritdoc />
    public partial class AddAvailableToAsset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssetName",
                table: "EmployeeAsset");

            migrationBuilder.DropColumn(
                name: "EmployeeName",
                table: "EmployeeAsset");

            migrationBuilder.AddColumn<bool>(
                name: "Available",
                table: "Asset",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAsset_AssetId",
                table: "EmployeeAsset",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAsset_EmployeeId",
                table: "EmployeeAsset",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAsset_Asset_AssetId",
                table: "EmployeeAsset",
                column: "AssetId",
                principalTable: "Asset",
                principalColumn: "AssetId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeAsset_Employee_EmployeeId",
                table: "EmployeeAsset",
                column: "EmployeeId",
                principalTable: "Employee",
                principalColumn: "EmployeeId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAsset_Asset_AssetId",
                table: "EmployeeAsset");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeAsset_Employee_EmployeeId",
                table: "EmployeeAsset");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAsset_AssetId",
                table: "EmployeeAsset");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeAsset_EmployeeId",
                table: "EmployeeAsset");

            migrationBuilder.DropColumn(
                name: "Available",
                table: "Asset");

            migrationBuilder.AddColumn<string>(
                name: "AssetName",
                table: "EmployeeAsset",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeName",
                table: "EmployeeAsset",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
