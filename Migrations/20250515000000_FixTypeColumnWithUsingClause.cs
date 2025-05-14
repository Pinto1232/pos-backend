using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PosBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixTypeColumnWithUsingClause : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL to apply the USING clause for the Type column conversion
            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    -- Check if the Scope table exists
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'scope') THEN
                        -- Check if Type column is text
                        IF EXISTS (
                            SELECT FROM information_schema.columns 
                            WHERE table_schema = 'public' 
                            AND table_name = 'scope' 
                            AND column_name = 'type' 
                            AND data_type = 'text'
                        ) THEN
                            -- Alter the Type column with a USING clause to convert string values to integers
                            ALTER TABLE ""Scope"" 
                            ALTER COLUMN ""Type"" TYPE integer 
                            USING CASE 
                                WHEN ""Type"" = 'Global' THEN 0
                                WHEN ""Type"" = 'Store' THEN 1
                                WHEN ""Type"" = 'Terminal' THEN 2
                                ELSE 0 -- Default to Global if unknown
                            END;
                            
                            RAISE NOTICE 'Successfully converted Type column from text to integer';
                        ELSE
                            RAISE NOTICE 'Type column is already an integer or does not exist';
                        END IF;
                    ELSE
                        RAISE NOTICE 'Scope table does not exist';
                    END IF;
                END
                $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This is a one-way migration, no down migration is provided
        }
    }
}
