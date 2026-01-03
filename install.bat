@echo off
title FootageSearch Installation - Geekatplay Studio
echo ========================================================
echo      FootageSearch Installer - Geekatplay Studio
echo ========================================================
echo.

:: Check for .NET SDK
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] .NET SDK is not installed or not in PATH.
    echo Please install .NET 10 SDK from https://dotnet.microsoft.com/
    pause
    exit /b 1
)
echo [OK] .NET SDK found.

:: Check for Node.js
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Node.js is not installed or not in PATH.
    echo Please install Node.js from https://nodejs.org/
    pause
    exit /b 1
)
echo [OK] Node.js found.

:: Check for Docker (Optional but recommended for Qdrant)
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo [WARNING] Docker is not found. You will need to run Qdrant manually.
) else (
    echo [OK] Docker found.
)

echo.
echo --------------------------------------------------------
echo 1. Building .NET Solution...
echo --------------------------------------------------------
dotnet build FootageSearch.sln -c Release
if %errorlevel% neq 0 (
    echo [ERROR] Build failed.
    pause
    exit /b 1
)

echo.
echo --------------------------------------------------------
echo 2. Installing Resolve Plugin Dependencies...
echo --------------------------------------------------------
cd FootageSearch.ResolvePlugin
call npm install
if %errorlevel% neq 0 (
    echo [ERROR] npm install failed.
    cd ..
    pause
    exit /b 1
)
cd ..

echo.
echo --------------------------------------------------------
echo 3. Setup Complete!
echo --------------------------------------------------------
echo.
echo To run the application:
echo 1. Ensure Qdrant is running (docker run -p 6333:6333 qdrant/qdrant).
echo 2. Ensure Ollama is running (ollama serve).
echo 3. Run the applications in this order:
echo    - FootageSearch.Api
echo    - FootageSearch.Indexer
echo    - FootageSearch.App
echo.
echo For the Resolve Plugin:
echo Copy "FootageSearch.ResolvePlugin" to:
echo "%%ProgramData%%\Blackmagic Design\DaVinci Resolve\Support\Workflow Integration Plugins\FootageSearch"
echo.
echo Thank you for using FootageSearch by Geekatplay Studio!
echo.
pause
