using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FootageSearch.Core.Services;
using FootageSearch.Data;
using FootageSearch.Embeddings.Services;
using FootageSearch.Indexer.Services;
using FootageSearch.Media.Services;
using FootageSearch.Search.Services;

using FootageSearch.Transcription.Services;
using FootageSearch.OCR.Services;
using FootageSearch.AI.Services;
using Microsoft.EntityFrameworkCore;

namespace FootageSearch.App
{
    public partial class MainWindow : Window
    {
        private IndexerService _indexerService = null!;
        private SearchService _searchService = null!;
        private VideoDbContext _dbContext = null!;
        private System.Windows.Threading.DispatcherTimer _statusTimer = null!;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            LoadAllFiles();
            SetupStatusPolling();
        }

        private void InitializeServices()
        {
            var settingsService = new JsonSettingsService();
            var settings = settingsService.LoadSettings();

            var mediaService = new FfmpegMediaService();
            var vectorDbService = new QdrantService();
            _dbContext = new VideoDbContext();
            var transcriptionService = new WhisperTranscriptionService();
            var ocrService = new TesseractOcrService();
            
            // Use OllamaVisualAiService with configured settings
            var visualAiService = new OllamaVisualAiService(settings.OllamaApiUrl, settings.OllamaModel);
            var embeddingService = new OllamaEmbeddingService(settings.OllamaApiUrl, settings.OllamaEmbeddingModel);

            _indexerService = new IndexerService(settingsService, mediaService, vectorDbService, transcriptionService, ocrService, visualAiService, embeddingService, _dbContext);
            _searchService = new SearchService(_dbContext, vectorDbService);
        }

        private void SetupStatusPolling()
        {
            _statusTimer = new System.Windows.Threading.DispatcherTimer();
            _statusTimer.Interval = TimeSpan.FromSeconds(1);
            _statusTimer.Tick += StatusTimer_Tick;
            _statusTimer.Start();
        }

        private async void StatusTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                // Use a separate context for polling to avoid concurrency issues
                using var context = new VideoDbContext();
                // We need to check if the database exists first
                if (!await context.Database.CanConnectAsync()) return;

                var status = await context.JobStatuses.FirstOrDefaultAsync(j => j.ServiceName == "Indexer");
                if (status != null)
                {
                    // Only update status text if we are not in the middle of a manual re-index (which shows the log panel)
                    if (LogPanel.Visibility != Visibility.Visible)
                    {
                        StatusText.Text = $"Indexer: [{status.CurrentTask}] {status.Detail}";
                    }
                }
            }
            catch { }
        }

        private async void LoadAllFiles()
        {
            try
            {
                var files = await _searchService.GetAllFilesAsync();
                ResultsList.ItemsSource = files;
            }
            catch (Exception)
            {
                // Database might not exist yet
                StatusText.Text = "Database empty or not initialized.";
            }
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            if (settingsWindow.ShowDialog() == true)
            {
                // Settings saved
            }
        }

        private async void ReIndex_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("This will rescan all watch folders. Continue?", "Re-Index", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            // Show Log Panel, Hide Results
            LogPanel.Visibility = Visibility.Visible;
            ResultsList.Visibility = Visibility.Collapsed;
            LogList.Items.Clear();
            
            LoadingOverlay.Visibility = Visibility.Visible;
            LoadingBar.IsIndeterminate = true;
            
            bool skipIndexed = SkipIndexedCheckBox.IsChecked ?? true;
            
            var progress = new Progress<string>(status => 
            {
                StatusText.Text = status;
                LogList.Items.Add($"[{DateTime.Now:HH:mm:ss}] {status}");
                if (LogList.Items.Count > 0)
                    LogList.ScrollIntoView(LogList.Items[LogList.Items.Count - 1]);
            });

            try
            {
                await Task.Run(() => _indexerService.ReIndexAsync(progress, skipIndexed));
                MessageBox.Show("Indexing Complete!");
                LoadAllFiles();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during indexing: {ex.Message}");
                LogList.Items.Add($"[ERROR] {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                // Keep LogPanel visible so user can see what happened, or switch back?
                // Let's switch back to results after a delay or user action. 
                // For now, let's hide log and show results to show the "data associated".
                LogPanel.Visibility = Visibility.Collapsed;
                ResultsList.Visibility = Visibility.Visible;
            }
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            await PerformSearch();
        }

        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await PerformSearch();
            }
        }

        private async Task PerformSearch()
        {
            var query = SearchBox.Text;
            if (string.IsNullOrWhiteSpace(query))
            {
                LoadAllFiles();
                return;
            }

            LoadingOverlay.Visibility = Visibility.Visible;
            StatusText.Text = "Searching...";

            try
            {
                var results = await _searchService.SearchAsync(query);
                ResultsList.ItemsSource = results;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search error: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Documentation_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Documentation not yet available.", "Help");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.Owner = this;
            aboutWindow.ShowDialog();
        }
    }
}