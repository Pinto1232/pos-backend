#!/bin/bash
# Get database connection details from appsettings.json
DB_HOST=$(grep -o '"Host=[^"]*' appsettings.json | cut -d'=' -f2)
DB_NAME=$(grep -o 'Database=[^;]*' appsettings.json | cut -d'=' -f2)
DB_USER=$(grep -o 'Username=[^;]*' appsettings.json | cut -d'=' -f2)
DB_PASS=$(grep -o 'Password=[^"]*' appsettings.json | cut -d'=' -f2)

echo "Seeding translations to database $DB_NAME on $DB_HOST..."
PGPASSWORD=$DB_PASS psql -h $DB_HOST -d $DB_NAME -U $DB_USER -f seed-translations.sql

if [ $? -eq 0 ]; then
  echo "Translations seeded successfully!"
else
  echo "Error seeding translations."
fi
