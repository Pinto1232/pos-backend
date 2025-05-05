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

echo "Removing all migrations..."
rm -rf Infrastructure/Data/Migrations/*

echo "Creating a new initial migration..."
dotnet ef migrations add InitialCreate

echo "Applying the migration..."
dotnet ef database update

echo "Database reset complete!"
