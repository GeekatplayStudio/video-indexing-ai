using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FootageSearch.Core.Interfaces;
using FootageSearch.Core.Models;
using FootageSearch.Core.Services;
using Microsoft.Win32;

namespace FootageSearch.App
{
    public partial class SettingsWindow : Window
    {
        private readonly ISettingsService _settingsService;
        private AppSettings _currentSettings = null!;

        public SettingsWindow()
        {
            InitializeComponent();
            _settingsService = new JsonSettingsService(); // In a real app, use DI
            LoadData();
        }

        private void LoadData()
        {
            // Load Settings
            _currentSettings = _settingsService.LoadSettings();
            
            // Populate Watch Folders
            WatchFoldersList.ItemsSource = null;
            WatchFoldersList.ItemsSource = _currentSettings.WatchFolders;

            // Populate Temp Path
            TempPathTextBox.Text = _currentSettings.TempFolderPath;

            // Populate AI Settings
            OllamaUrlTextBox.Text = _currentSettings.OllamaApiUrl;
            OllamaModelTextBox.Text = _currentSettings.OllamaModel;
            OllamaEmbeddingModelTextBox.Text = _currentSettings.OllamaEmbeddingModel;
            
            foreach (ComboBoxItem item in WhisperModelComboBox.Items)
            {
                if (item.Content.ToString() == _currentSettings.WhisperModelType)
                {
                    WhisperModelComboBox.SelectedItem = item;
                    break;
                }
            }

            // Populate Drives
            LoadDrives();
        }

        private void LoadDrives()
        {
            try
            {
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => $"{d.Name} ({d.VolumeLabel})").ToList();
                DrivesComboBox.ItemsSource = drives;
                if (drives.Any())
                {
                    DrivesComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading drives: {ex.Message}");
            }
        }

        private void DrivesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Optional: Show drive details
        }

        private void AddDrive_Click(object sender, RoutedEventArgs e)
        {
            if (DrivesComboBox.SelectedItem is string driveStr)
            {
                var driveLetter = driveStr.Split(' ')[0]; // Get "C:\"
                if (!_currentSettings.WatchFolders.Contains(driveLetter))
                {
                    _currentSettings.WatchFolders.Add(driveLetter);
                    RefreshList();
                }
            }
        }

        private void AddFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Select Watch Folder";
            if (dialog.ShowDialog() == true)
            {
                var path = dialog.FolderName;
                if (!_currentSettings.WatchFolders.Contains(path))
                {
                    _currentSettings.WatchFolders.Add(path);
                    RefreshList();
                }
            }
        }

        private void RemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            if (WatchFoldersList.SelectedItem is string selectedPath)
            {
                _currentSettings.WatchFolders.Remove(selectedPath);
                RefreshList();
            }
        }

        private void BrowseTemp_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog();
            dialog.Title = "Select Temporary Directory";
            if (dialog.ShowDialog() == true)
            {
                _currentSettings.TempFolderPath = dialog.FolderName;
                TempPathTextBox.Text = dialog.FolderName;
            }
        }

        private void RefreshList()
        {
            WatchFoldersList.ItemsSource = null;
            WatchFoldersList.ItemsSource = _currentSettings.WatchFolders;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            _currentSettings.OllamaApiUrl = OllamaUrlTextBox.Text;
            _currentSettings.OllamaModel = OllamaModelTextBox.Text;
            _currentSettings.OllamaEmbeddingModel = OllamaEmbeddingModelTextBox.Text;
            if (WhisperModelComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Content != null)
            {
                _currentSettings.WhisperModelType = selectedItem.Content.ToString() ?? "base";
            }

            _settingsService.SaveSettings(_currentSettings);
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
