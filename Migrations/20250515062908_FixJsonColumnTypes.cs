using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixJsonColumnTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.AlterColumn<string>(
            //     name: "TaxSettingsJson",
            //     table: "UserCustomizations",
            //     type: "jsonb",
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "text",
            //     oldNullable: true);

            migrationBuilder.Sql(@"
                ALTER TABLE ""UserCustomizations""
                ALTER COLUMN ""TaxSettingsJson"" TYPE jsonb
                USING CASE
                    WHEN ""TaxSettingsJson"" IS NULL OR TRIM(""TaxSettingsJson"") = '' THEN NULL
                    ELSE ""TaxSettingsJson""::jsonb
                END;
            ");

            // migrationBuilder.AlterColumn<string>(
            //     name: "RegionalSettingsJson",
            //     table: "UserCustomizations",
            //     type: "jsonb",
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "text",
            //     oldNullable: true);

            migrationBuilder.Sql(@"
                ALTER TABLE ""UserCustomizations""
                ALTER COLUMN ""RegionalSettingsJson"" TYPE jsonb
                USING CASE
                    WHEN ""RegionalSettingsJson"" IS NULL OR TRIM(""RegionalSettingsJson"") = '' THEN NULL
                    ELSE ""RegionalSettingsJson""::jsonb
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TaxSettingsJson",
                table: "UserCustomizations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RegionalSettingsJson",
                table: "UserCustomizations",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
