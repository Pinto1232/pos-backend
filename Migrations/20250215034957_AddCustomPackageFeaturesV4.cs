using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomPackageFeaturesV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Dependencies",
                table: "AddOns");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<int>>(
                name: "Dependencies",
                table: "AddOns",
                type: "integer[]",
                nullable: false);

            migrationBuilder.UpdateData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 201,
                column: "Dependencies",
                value: new List<int>());

            migrationBuilder.UpdateData(
                table: "AddOns",
                keyColumn: "Id",
                keyValue: 202,
                column: "Dependencies",
                value: new List<int>());
        }
    }
}
