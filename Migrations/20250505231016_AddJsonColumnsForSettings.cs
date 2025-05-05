using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddJsonColumnsForSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxSettings",
                table: "UserCustomizations",
                newName: "TaxSettingsJson");

            migrationBuilder.RenameColumn(
                name: "RegionalSettings",
                table: "UserCustomizations",
                newName: "RegionalSettingsJson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TaxSettingsJson",
                table: "UserCustomizations",
                newName: "TaxSettings");

            migrationBuilder.RenameColumn(
                name: "RegionalSettingsJson",
                table: "UserCustomizations",
                newName: "RegionalSettings");
        }
    }
}
