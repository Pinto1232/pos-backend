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
            // Log distinct values from Scope.Type before attempting conversion
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    type_value TEXT;
                BEGIN
                    IF EXISTS (SELECT FROM information_schema.tables WHERE table_schema = 'public' AND table_name = 'scope') THEN
                        IF EXISTS (
                            SELECT FROM information_schema.columns
                            WHERE table_schema = 'public'
                            AND table_name = 'scope'
                            AND column_name = 'type'
                            AND (data_type = 'text' OR data_type = 'character varying') -- Include varchar just in case
                        ) THEN
                            RAISE NOTICE 'Distinct values in Scope.Type before conversion:';
                            FOR type_value IN EXECUTE 'SELECT DISTINCT ""Type"" FROM ""Scope""'
                            LOOP
                                RAISE NOTICE 'Value: %', type_value;
                            END LOOP;
                        ELSE
                            RAISE NOTICE 'Scope.Type column is not text/varchar or does not exist.';
                        END IF;
                    ELSE
                        RAISE NOTICE 'Scope table does not exist, skipping logging of Scope.Type values.';
                    END IF;
                END
                $$;
            ");

            // Original SQL to apply the USING clause for the Type column conversion
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
                            AND (data_type = 'text' OR data_type = 'character varying')
                        ) THEN
                            -- Alter the Type column with a USING clause to convert string values to integers
                            ALTER TABLE ""Scope""
                            ALTER COLUMN ""Type"" TYPE integer
                            USING CASE
                                WHEN TRIM(BOTH ' ' FROM ""Type"") = 'Global' THEN 0
                                WHEN TRIM(BOTH ' ' FROM ""Type"") = 'Store' THEN 1
                                WHEN TRIM(BOTH ' ' FROM ""Type"") = 'Terminal' THEN 2
                                WHEN ""Type"" IS NULL THEN 0 -- Explicitly handle NULL
                                WHEN TRIM(BOTH ' ' FROM ""Type"") = '' THEN 0 -- Handle empty strings
                                ELSE 0 -- Default to Global if unknown
                            END;
                            
                            RAISE NOTICE 'Successfully converted Type column from text to integer';
                        ELSE
                            RAISE NOTICE 'Type column is already an integer or does not exist, or table was not text/varchar.';
                        END IF;
                    ELSE
                        RAISE NOTICE 'Scope table does not exist, skipping conversion.';
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
