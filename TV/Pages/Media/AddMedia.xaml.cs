using Microsoft.WindowsAPICodePack.Shell;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using TV.Classes;

namespace TV.Pages.Media
{
    /// <summary>
    /// Логика взаимодействия для AddMedia.xaml
    /// </summary>
    public partial class AddMedia : Page
    {
        private List<PlaylistItem> selectedFiles = new List<PlaylistItem>();
        private Playlist selectedPlaylist;
        public AddMedia(Playlist selectedPlaylist)
        {
            InitializeComponent();

            this.selectedPlaylist = selectedPlaylist;
        }

        private async Task<bool> AddFilesToPlaylistAsync(int playlistId, List<PlaylistItem> files)
        {
            using (var connection = new MySqlConnection(Classes.Common.Connection.connectionString))
            {
                await connection.OpenAsync();

                var maxOrderQuery = "SELECT COALESCE(MAX(`Order`), 0) FROM PlaylistItems WHERE PlaylistId = @playlistId";
                using (var maxOrderCommand = new MySqlCommand(maxOrderQuery, connection))
                {
                    maxOrderCommand.Parameters.AddWithValue("@playlistId", playlistId);
                    var currentMaxOrder = Convert.ToInt32(await maxOrderCommand.ExecuteScalarAsync());
                    var orderIndex = currentMaxOrder + 1;

                    var insertQuery = @"INSERT INTO PlaylistItems 
                                    (PlaylistId, `Order`, Name, Type, Duration, Size, FilePath) 
                                    VALUES (@playlistId, @orderIndex, @name, @type, @duration, @size, @filePath)";

                    foreach (var file in files)
                    {
                        using (var insertCommand = new MySqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@playlistId", playlistId);
                            insertCommand.Parameters.AddWithValue("@orderIndex", orderIndex++);
                            insertCommand.Parameters.AddWithValue("@name", file.Name);
                            insertCommand.Parameters.AddWithValue("@type", file.Type);
                            insertCommand.Parameters.AddWithValue("@duration", file.Duration);
                            insertCommand.Parameters.AddWithValue("@size", file.Size);
                            insertCommand.Parameters.AddWithValue("@filePath", file.FilePath);

                            await insertCommand.ExecuteNonQueryAsync();
                        }
                    }
                }
                return true;
            }
        }

        private void BrowseMediaFiles(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "Все поддерживаемые форматы|*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp;|" +
                "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp;*.tiff;*.svg|" +
                "Видео файлы|*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.flv;*.webm;*.m4v|" +
                 "Все файлы|*.*",
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var order = 1;

                foreach (var filePath in openFileDialog.FileNames)
                {
                    var fileInfo = new FileInfo(filePath);
                    var extension = Path.GetExtension(filePath).ToLower().TrimStart('.');
                    var mediaType = GetMediaType(extension);

                    var duration = 0;
                    if (mediaType == "video")
                        duration = GetVideoDuration(filePath);
                        
                    selectedFiles.Add(new PlaylistItem
                    {
                        Name = Path.GetFileNameWithoutExtension(filePath),
                        Type = mediaType,
                        Order = order,
                        FilePath = filePath,
                        Size = fileInfo.Length,
                        Duration = duration
                    });

                    order++;
                }

                UpdateUI();
            }
        }

        private int GetVideoDuration(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return 30;

                using (var shellFile = ShellObject.FromParsingName(filePath))
                {
                    var durationProp = shellFile.Properties.System.Media.Duration;
                    if (durationProp?.Value != null)
                    {
                        ulong durationTicks = Convert.ToUInt64(durationProp.Value);
                        int durationSeconds = (int)(durationTicks / 10000000);

                        if (durationSeconds > 0 && durationSeconds < 86400)
                        {
                            return durationSeconds;
                        }
                    }
                }
            }
            catch {}

            return 30;
        }

        private static readonly Dictionary<string, string> mediaTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"mp4", "video"}, {"avi", "video"}, {"mkv", "video"}, {"mov", "video"},
            {"wmv", "video"}, {"flv", "video"}, {"webm", "video"},
          
            {"jpg", "image"}, {"jpeg", "image"}, {"png", "image"}, {"gif", "image"},
            {"bmp", "image"}, {"tiff", "image"}, {"webp", "image"}
        };

        private string GetMediaType(string extension)
        {
            if (mediaTypes.TryGetValue(extension, out string mediaType))
                return mediaType;

            return "unknown";
        }

        private void UpdateUI()
        {
            selectedFilesList.ItemsSource = null;
            selectedFilesList.ItemsSource = selectedFiles;

            emptyFilesText.Visibility = selectedFiles.Any() ? Visibility.Collapsed : Visibility.Visible;

            if (selectedFiles.Any())
            {
                var totalSize = selectedFiles.Sum(f => f.Size);
                var fileCount = selectedFiles.Count;

                selectionInfo.Text = $"Выбрано файлов: {fileCount}";
            }
            else
            {
                selectionInfo.Text = "Выберите файлы для добавления в плейлист";
            }
        }

        private async void AddToPlaylist(object sender, RoutedEventArgs e)
        {
            if (!selectedFiles.Any())
            {
                ShowWarning("Выберите файлы для добавления");
                return;
            }

            try
            {
                SetAddButtonState(isEnabled: false, text: "Добавление...");

                var success = await AddFilesToPlaylistAsync(selectedPlaylist.Id, selectedFiles);

                if (success)
                {
                    ShowSuccess($"Файлы успешно добавлены в плейлист '{selectedPlaylist.Name}'");
                    selectedFiles.Clear();
                    UpdateUI();
                }
                else
                {
                    ShowError("Ошибка при добавлении файлов");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка: {ex.Message}");
            }
            finally
            {
                SetAddButtonState(isEnabled: true, text: "Добавить в плейлист");
            }
        }

        private void ShowError(string message)
        => MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);

        private void ShowWarning(string message)
            => MessageBox.Show(message, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);

        private void ShowSuccess(string message)
            => MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

        private void SetAddButtonState(bool isEnabled, string text)
        {
            addButton.IsEnabled = isEnabled;
            addButton.Content = text;
        }

        private void CloseWindow(object sender, RoutedEventArgs e)
        {

        }

        private void ContentType_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void RemoveFile(object sender, RoutedEventArgs e)
        {

        }

        private void BrowseMediaFolder(object sender, RoutedEventArgs e)
        {

        }

        private void AddWebUrl(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveWebUrl(object sender, RoutedEventArgs e)
        {

        }

        private void BrowseFolder(object sender, RoutedEventArgs e)
        {

        }

        private void Duration_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
    }

    public class VideoToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type == "video" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
