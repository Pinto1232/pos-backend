#!/bin/bash

echo "Dropping database if it exists..."
dotnet ef database drop --force

echo "Creating a new database..."
# Extract connection string from appsettings.json
CONNECTION_STRING=$(grep -o '"DefaultConnection": "[^"]*"' appsettings.json | cut -d'"' -f4)
DB_HOST=$(echo $CONNECTION_STRING | grep -o 'Host=[^;]*' | cut -d'=' -f2)
DB_NAME=$(echo $CONNECTION_STRING | grep -o 'Database=[^;]*' | cut -d'=' -f2)
DB_USER=$(echo $CONNECTION_STRING | grep -o 'Username=[^;]*' | cut -d'=' -f2)
DB_PASS=$(echo $CONNECTION_STRING | grep -o 'Password=[^;]*' | cut -d'=' -f2)

# Create database using psql
PGPASSWORD=$DB_PASS psql -h $DB_HOST -U $DB_USER -c "CREATE DATABASE $DB_NAME;"

echo "Creating tables manually..."
cat << EOF > create-tables.sql
-- Create basic tables first
CREATE TABLE "Stores" (
    "StoreId" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Address" VARCHAR(255),
    "PhoneNumber" VARCHAR(20)
);

CREATE TABLE "Categories" (
    "CategoryId" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "ParentCategoryId" INTEGER NULL,
    CONSTRAINT "FK_Categories_Categories_ParentCategoryId" FOREIGN KEY ("ParentCategoryId") REFERENCES "Categories" ("CategoryId") ON DELETE NO ACTION
);

CREATE TABLE "Suppliers" (
    "SupplierId" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "ContactName" VARCHAR(100),
    "Email" VARCHAR(100),
    "Phone" VARCHAR(20),
    "Address" VARCHAR(255),
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "LastUpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedAt" TIMESTAMP WITH TIME ZONE NULL
);

-- Now create tables that depend on the above
CREATE TABLE "Products" (
    "ProductId" SERIAL PRIMARY KEY,
    "CategoryId" INTEGER NOT NULL,
    "SupplierId" INTEGER NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500) NOT NULL DEFAULT '',
    "SKU" VARCHAR(50),
    "Barcode" VARCHAR(50),
    "BasePrice" DECIMAL(18,2) NOT NULL,
    "TaxRate" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "LastUpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "IsDeleted" BOOLEAN NOT NULL DEFAULT FALSE,
    "DeletedAt" TIMESTAMP WITH TIME ZONE NULL,
    CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES "Categories" ("CategoryId") ON DELETE CASCADE,
    CONSTRAINT "FK_Products_Suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES "Suppliers" ("SupplierId") ON DELETE CASCADE
);

CREATE TABLE "ProductVariants" (
    "VariantId" SERIAL PRIMARY KEY,
    "ProductId" INTEGER NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "SKU" VARCHAR(50),
    "Barcode" VARCHAR(50),
    "Price" DECIMAL(18,2) NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT "FK_ProductVariants_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES "Products" ("ProductId") ON DELETE CASCADE
);

CREATE TABLE "Inventories" (
    "InventoryId" SERIAL PRIMARY KEY,
    "VariantId" INTEGER NOT NULL,
    "StoreId" INTEGER NOT NULL,
    "Quantity" INTEGER NOT NULL,
    "ReorderLevel" INTEGER NOT NULL,
    "LastUpdated" TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT "FK_Inventories_ProductVariants_VariantId" FOREIGN KEY ("VariantId") REFERENCES "ProductVariants" ("VariantId") ON DELETE CASCADE,
    CONSTRAINT "FK_Inventories_Stores_StoreId" FOREIGN KEY ("StoreId") REFERENCES "Stores" ("StoreId") ON DELETE CASCADE
);

-- Create __EFMigrationsHistory table
CREATE TABLE "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Insert the migration record
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20250421201340_AddedScopeEntity', '9.0.4');
EOF

# Execute the SQL script
PGPASSWORD=$DB_PASS psql -h $DB_HOST -U $DB_USER -d $DB_NAME -f create-tables.sql

echo "Applying remaining migrations..."
dotnet ef database update

echo "Database fixed!"
