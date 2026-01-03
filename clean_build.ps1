Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" } | Stop-Process -Force
Get-Process | Where-Object { $_.ProcessName -like "*FootageSearch*" } | Stop-Process -Force

Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.Api\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.Api\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.Indexer\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.Indexer\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.Data\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.Data\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.Core\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.Core\obj" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.App\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "d:\AI-video-orginizer\FootageSearch.App\obj" -Recurse -Force -ErrorAction SilentlyContinue

Remove-Item "$env:LOCALAPPDATA\FootageSearch\footage.db" -Force -ErrorAction SilentlyContinue

dotnet build FootageSearch.sln
