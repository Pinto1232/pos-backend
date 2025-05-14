#!/bin/bash
echo "Cleaning the project..."
dotnet clean PosBackend.csproj

echo "Building the project..."
dotnet build PosBackend.csproj

echo "Running the backend..."
dotnet run --project PosBackend.csproj dev
