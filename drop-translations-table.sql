-- Drop the Translations table if it exists
DROP TABLE IF EXISTS "Translations";

-- Remove the migration entries related to Translations
DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250505114203_AddTranslationsTable';
DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250505114500_SeedInitialTranslations';
DELETE FROM "__EFMigrationsHistory" WHERE "MigrationId" LIKE '%RemoveTranslationsTable%';
