using FootageSearch.Core.Models;

namespace FootageSearch.Core.Interfaces
{
    public interface ISettingsService
    {
        AppSettings LoadSettings();
        void SaveSettings(AppSettings settings);
    }
}
