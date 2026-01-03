# start.ps1
Write-Host "Starting FootageSearch System..." -ForegroundColor Cyan

# Function to check port
function Test-Port($port) {
    $tcp = New-Object System.Net.Sockets.TcpClient
    try {
        $tcp.Connect('localhost', $port)
        $tcp.Close()
        return $true
    } catch {
        return $false
    }
}

# 1. Qdrant Check
Write-Host "Checking Qdrant (Vector DB)..." -NoNewline
if (Test-Port 6333) {
    Write-Host " [OK]" -ForegroundColor Green
} else {
    Write-Host " [Not Running]" -ForegroundColor Yellow
    
    # Try to find local Qdrant
    if (Test-Path ".\Qdrant\qdrant.exe") {
        Write-Host "Starting local Qdrant..."
        $qdrantProcess = Start-Process -FilePath ".\Qdrant\qdrant.exe" -PassThru -WindowStyle Hidden
        # Wait a bit for it to start
        Start-Sleep -Seconds 5
    } else {
        # Check if setup script exists
        if (Test-Path ".\setup_qdrant.ps1") {
            Write-Host "Qdrant not found. Running setup..."
            .\setup_qdrant.ps1
            if (Test-Path ".\Qdrant\qdrant.exe") {
                Write-Host "Starting local Qdrant..."
                $qdrantProcess = Start-Process -FilePath ".\Qdrant\qdrant.exe" -PassThru -WindowStyle Hidden
                Start-Sleep -Seconds 5
            } else {
                Write-Host "Failed to setup Qdrant. Please check logs." -ForegroundColor Red
                exit
            }
        } else {
            Write-Host "Qdrant not found and setup script missing." -ForegroundColor Red
            exit
        }
    }
}

# 2. Ollama Check
Write-Host "Checking Ollama (AI Service)..." -NoNewline
if (Test-Port 11434) {
    Write-Host " [OK]" -ForegroundColor Green
} else {
    Write-Host " [Not Running]" -ForegroundColor Yellow
    Write-Host "Attempting to start Ollama..."
    # Try to start ollama serve
    try {
        $ollamaProcess = Start-Process -FilePath "ollama" -ArgumentList "serve" -PassThru -WindowStyle Hidden
        Start-Sleep -Seconds 5
    } catch {
        Write-Host "Could not start Ollama. Is it installed?" -ForegroundColor Red
        Write-Host "Please install Ollama from https://ollama.com"
        # We might continue, but AI features won't work
    }
}

# 3. Start Application Components
Write-Host "Starting FootageSearch Components..."

# Find dotnet
$dotnet = "dotnet"
if (Get-Command "dotnet" -ErrorAction SilentlyContinue) {
    $dotnet = "dotnet"
} elseif (Test-Path "C:\Program Files\dotnet\dotnet.exe") {
    $dotnet = "C:\Program Files\dotnet\dotnet.exe"
}

# Start API
Write-Host "Launching API..."
$apiProcess = Start-Process -FilePath $dotnet -ArgumentList "run --project FootageSearch.Api/FootageSearch.Api.csproj --urls http://localhost:5001" -PassThru -WindowStyle Hidden

# Start Indexer
Write-Host "Launching Indexer..."
$indexerProcess = Start-Process -FilePath $dotnet -ArgumentList "run --project FootageSearch.Indexer/FootageSearch.Indexer.csproj" -PassThru -WindowStyle Hidden

# Start App (Blocking? No, we want to monitor)
Write-Host "Launching App..."
$appProcess = Start-Process -FilePath $dotnet -ArgumentList "run --project FootageSearch.App/FootageSearch.App.csproj" -PassThru

Write-Host "System Running. Close the App window to shut down all services." -ForegroundColor Green

# Monitor App
if ($appProcess) {
    $appProcess.WaitForExit()
}

# Cleanup
Write-Host "Shutting down services..."
if ($apiProcess -and !$apiProcess.HasExited) { Stop-Process -Id $apiProcess.Id -Force -ErrorAction SilentlyContinue }
if ($indexerProcess -and !$indexerProcess.HasExited) { Stop-Process -Id $indexerProcess.Id -Force -ErrorAction SilentlyContinue }
if ($qdrantProcess -and !$qdrantProcess.HasExited) { Stop-Process -Id $qdrantProcess.Id -Force -ErrorAction SilentlyContinue }
if ($ollamaProcess -and !$ollamaProcess.HasExited) { Stop-Process -Id $ollamaProcess.Id -Force -ErrorAction SilentlyContinue }

Write-Host "Shutdown Complete."
