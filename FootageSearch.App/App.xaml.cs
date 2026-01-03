using System.Windows;

namespace FootageSearch.App
{
    public partial class App : Application
    {
        protected override void OnExit(ExitEventArgs e)
        {
            ProcessManager.ShutdownServices();
            base.OnExit(e);
        }
    }
}