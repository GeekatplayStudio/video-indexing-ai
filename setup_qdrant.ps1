$url = "https://github.com/qdrant/qdrant/releases/download/v1.13.0/qdrant-x86_64-pc-windows-msvc.zip"
$output = "qdrant.zip"
$dest = "Qdrant"

Write-Host "Downloading Qdrant..."
Invoke-WebRequest -Uri $url -OutFile $output

Write-Host "Extracting..."
Expand-Archive -Path $output -DestinationPath $dest -Force

Write-Host "Cleaning up..."
Remove-Item $output

Write-Host "Qdrant setup complete. Run ./Qdrant/qdrant.exe to start the server."
