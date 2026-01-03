using System;
using System.Diagnostics;
using System.IO;

namespace FootageSearch.App
{
    public static class ProcessManager
    {
        private static Process? _apiProcess;
        private static Process? _indexerProcess;

        public static void StartServices()
        {
            // Assuming we are running from the build output, the other executables should be nearby
            // Or we are running in dev mode.
            // Let's try to find them relative to the current directory.
            
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            // In Debug/Release, they might be in parallel folders if not published.
            // But if published, they are in the same folder.
            
            // Strategy: Look for .exe or .dll
            // For dev environment (dotnet run), it's harder.
            // But the user asked to "start test app" and "we have a lot of .net host running".
            // This implies they are running via dotnet run or similar.
            
            // If we are in VS Code dev environment, we might want to skip this if the user launches them manually.
            // BUT the user specifically asked to avoid orphaned processes.
            
            // Let's assume we are in a deployed scenario OR we want to launch them if not running.
            // For now, let's implement the cleanup logic primarily.
            
            // If we want to launch them:
            // StartProcess("FootageSearch.Api.exe");
            // StartProcess("FootageSearch.Indexer.exe");
        }

        public static void ShutdownServices()
        {
            KillProcess("FootageSearch.Api");
            KillProcess("FootageSearch.Indexer");
            
            // Also kill any dotnet processes that might be ours (risky, but requested)
            // "be sure we dont creating orphanc processing"
            // We can't easily know which dotnet process is ours without tracking PIDs.
            
            if (_apiProcess != null && !_apiProcess.HasExited)
            {
                try { _apiProcess.Kill(); } catch { }
            }

            if (_indexerProcess != null && !_indexerProcess.HasExited)
            {
                try { _indexerProcess.Kill(); } catch { }
            }
        }

        private static void KillProcess(string processName)
        {
            foreach (var process in Process.GetProcessesByName(processName))
            {
                try
                {
                    process.Kill();
                    process.WaitForExit(1000);
                }
                catch { }
            }
        }
    }
}
