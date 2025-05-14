using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class EnhanceAddOnDataStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Dependencies",
                table: "AddOns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "AddOns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "AddOns",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dependencies",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "Features",
                table: "AddOns");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "AddOns");
        }
    }
}
