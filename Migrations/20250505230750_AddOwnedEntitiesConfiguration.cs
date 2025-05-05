using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnedEntitiesConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegionalSettings",
                table: "UserCustomizations",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxSettings",
                table: "UserCustomizations",
                type: "jsonb",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegionalSettings",
                table: "UserCustomizations");

            migrationBuilder.DropColumn(
                name: "TaxSettings",
                table: "UserCustomizations");
        }
    }
}
