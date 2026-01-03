# FootageSearch by Geekatplay Studio

Local media indexing/search application with DaVinci Resolve Studio 20 integration.
Developed by **Geekatplay Studio**.

## Structure

- **FootageSearch.App**: WPF Desktop Application
- **FootageSearch.Indexer**: Background Worker Service for indexing
- **FootageSearch.Api**: Local API for search and integration
- **FootageSearch.ResolvePlugin**: Electron-based Workflow Integration plugin for DaVinci Resolve
- **FootageSearch.ResolveScript**: Python helper scripts

## Prerequisites

- Visual Studio 2022
- .NET 10 SDK (x64)
- Node.js (x64)
- Docker Desktop (for Qdrant) OR Local Qdrant Binary
- ffmpeg/ffprobe (Must be in PATH)
- Ollama (for Visual AI)

## AI Setup

1. **Ollama**: Install from [ollama.com](https://ollama.com).
   - Run `ollama pull llama3.2-vision` (or your preferred vision model).
   - Run `ollama pull nomic-embed-text` (for embeddings).
   - Ensure Ollama is running (`ollama serve`).
2. **Whisper**: The application will automatically download the `ggml-base.bin` model to `%AppData%\FootageSearch\Models` on first use.
3. **Tesseract**: The application will automatically download `eng.traineddata` to `%AppData%\FootageSearch\tessdata` on first use.

## Build Instructions

1. Open `FootageSearch.sln` in Visual Studio.
2. Build the solution.

## Running

1. Run `start.bat` to launch the entire system (checks dependencies, starts Qdrant/Ollama, and launches the app).
2. Alternatively:
   - Start Qdrant (Docker or Local).
   - Run `FootageSearch.Api`.
   - Run `FootageSearch.Indexer`.
   - Run `FootageSearch.App`.

## Resolve Plugin

1. Navigate to `FootageSearch.ResolvePlugin`.
2. Run `npm install`.
3. Run `npm start` to test or `npm run build` to build.
4. Copy the folder to `C:\ProgramData\Blackmagic Design\DaVinci Resolve\Support\Workflow Integration Plugins\FootageSearch`.