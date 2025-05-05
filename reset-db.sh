#!/bin/bash

echo -e "\e[36mResetting database and migrations...\e[0m"

# 1. Drop the database
echo -e "\e[33mDropping the database...\e[0m"
dotnet ef database drop --force

# 2. Remove all existing migrations
echo -e "\e[33mRemoving existing migrations...\e[0m"
MIGRATIONS_DIR="Infrastructure/Data/Migrations"
if [ -d "$MIGRATIONS_DIR" ]; then
    rm -rf "$MIGRATIONS_DIR"/*
fi

# 3. Create a new initial migration
echo -e "\e[33mCreating a new initial migration...\e[0m"
dotnet ef migrations add InitialCreate

# 4. Apply the migration
echo -e "\e[33mApplying the migration...\e[0m"
dotnet ef database update

echo -e "\e[32mDatabase reset complete!\e[0m"
