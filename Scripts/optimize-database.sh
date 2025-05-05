#!/bin/bash

echo "Optimizing database performance..."

# Use the Windows-style path for PostgreSQL
PSQL_BIN="/c/Program\ Files/PostgreSQL/16/bin/psql.exe"

# Check if the PostgreSQL executable exists
if [ ! -f "${PSQL_BIN//\\}" ]; then
    echo "PostgreSQL not found at expected location: ${PSQL_BIN//\\}"
    echo "Trying to find PostgreSQL in common locations..."

    # Try to find PostgreSQL in common locations
    for version in 16 15 14 13 12; do
        test_path="/c/Program\ Files/PostgreSQL/$version/bin/psql.exe"
        if [ -f "${test_path//\\}" ]; then
            PSQL_BIN="$test_path"
            echo "Found PostgreSQL $version"
            break
        fi
    done

    # If still not found, try to use psql from PATH
    if [ ! -f "${PSQL_BIN//\\}" ]; then
        if command -v psql >/dev/null 2>&1; then
            echo "Using psql from PATH"
            PSQL_BIN="psql"
        else
            echo "PostgreSQL not found. Please install PostgreSQL or update the script."
            exit 1
        fi
    fi
fi

# Extract connection string from appsettings.json
CONNECTION_STRING=$(grep -o '"DefaultConnection": "[^"]*"' ../appsettings.json | cut -d'"' -f4)
DB_HOST=$(echo $CONNECTION_STRING | grep -o 'Host=[^;]*' | cut -d'=' -f2)
DB_NAME=$(echo $CONNECTION_STRING | grep -o 'Database=[^;]*' | cut -d'=' -f2)
DB_USER=$(echo $CONNECTION_STRING | grep -o 'Username=[^;]*' | cut -d'=' -f2)
DB_PASS=$(echo $CONNECTION_STRING | grep -o 'Password=[^;]*' | cut -d'=' -f2)

echo "Database connection details:"
echo "Host: $DB_HOST"
echo "Database: $DB_NAME"
echo "User: $DB_USER"

# Run the index creation script
echo "Creating indexes..."
export PGPASSWORD="$DB_PASS"

if [ "$PSQL_BIN" = "psql" ]; then
    # Using psql from PATH
    psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -f CreateIndexes.sql
else
    # Using full path with escaped spaces
    eval "${PSQL_BIN} -h \"$DB_HOST\" -U \"$DB_USER\" -d \"$DB_NAME\" -f CreateIndexes.sql"
fi

# Vacuum analyze to update statistics
echo "Running VACUUM ANALYZE to update statistics..."
if [ "$PSQL_BIN" = "psql" ]; then
    # Using psql from PATH
    psql -h "$DB_HOST" -U "$DB_USER" -d "$DB_NAME" -c "VACUUM ANALYZE;"
else
    # Using full path with escaped spaces
    eval "${PSQL_BIN} -h \"$DB_HOST\" -U \"$DB_USER\" -d \"$DB_NAME\" -c \"VACUUM ANALYZE;\""
fi

# Clear password from environment
unset PGPASSWORD

echo "Database optimization complete!"
