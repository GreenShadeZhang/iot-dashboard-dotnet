#!/bin/bash
# Build script for IoT Dashboard (Linux/macOS)

echo "==================================="
echo "IoT Dashboard Build Script"
echo "==================================="
echo ""

# Check if dotnet is installed
if ! command -v dotnet &> /dev/null; then
    echo "Error: .NET SDK not found. Please install .NET 8.0 SDK."
    exit 1
fi

echo "Step 1: Restoring NuGet packages..."
dotnet restore
if [ $? -ne 0 ]; then
    echo "Error: Failed to restore packages."
    exit 1
fi
echo ""

echo "Step 2: Building Core library..."
dotnet build src/IotDashboard.Core/IotDashboard.Core.csproj -c Release
if [ $? -ne 0 ]; then
    echo "Error: Failed to build Core library."
    exit 1
fi
echo ""

echo "Note: WinUI application can only be built on Windows."
echo "Core library built successfully!"
echo ""

echo "==================================="
echo "Build completed!"
echo "==================================="
echo ""
