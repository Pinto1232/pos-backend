#!/bin/bash
# Get database connection details from appsettings.json
DB_HOST=$(grep -o '"Host=[^"]*' appsettings.json | cut -d'=' -f2)
DB_NAME=$(grep -o 'Database=[^;]*' appsettings.json | cut -d'=' -f2)
DB_USER=$(grep -o 'Username=[^;]*' appsettings.json | cut -d'=' -f2)
DB_PASS=$(grep -o 'Password=[^"]*' appsettings.json | cut -d'=' -f2)

echo "Dropping Translations table from database $DB_NAME on $DB_HOST..."
PGPASSWORD=$DB_PASS psql -h $DB_HOST -d $DB_NAME -U $DB_USER -f drop-translations-table.sql

if [ $? -eq 0 ]; then
  echo "Translations table dropped successfully!"
else
  echo "Error dropping Translations table."
fi
