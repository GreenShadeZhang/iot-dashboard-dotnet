@echo off
REM Build script for IoT Dashboard

echo ===================================
echo IoT Dashboard Build Script
echo ===================================
echo.

REM Check if dotnet is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Error: .NET SDK not found. Please install .NET 8.0 SDK.
    exit /b 1
)

echo Step 1: Restoring NuGet packages...
dotnet restore
if %errorlevel% neq 0 (
    echo Error: Failed to restore packages.
    exit /b 1
)
echo.

echo Step 2: Building Core library...
dotnet build src\IotDashboard.Core\IotDashboard.Core.csproj -c Release
if %errorlevel% neq 0 (
    echo Error: Failed to build Core library.
    exit /b 1
)
echo.

echo Step 3: Building WinUI application...
dotnet build src\IotDashboard.App\IotDashboard.App.csproj -c Release
if %errorlevel% neq 0 (
    echo Error: Failed to build application.
    echo Note: WinUI apps require Windows 10/11 to build.
    exit /b 1
)
echo.

echo ===================================
echo Build completed successfully!
echo ===================================
echo.
echo Output location: src\IotDashboard.App\bin\Release
echo.
