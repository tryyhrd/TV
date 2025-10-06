using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TV.Classes;

namespace TV.Pages.Media
{
    /// <summary>
    /// Логика взаимодействия для AddMedia.xaml
    /// </summary>
    public partial class AddMedia : Page
    {
        private List<ContentItem> selectedFiles = new List<ContentItem>();
        private Playlist selectedPlaylist;
        public AddMedia(Playlist selectedPlaylist)
        {
            InitializeComponent();

            this.selectedPlaylist = selectedPlaylist;
        }

        private async Task<bool> AddFilesToPlaylistAsync(int playlistId, List<ContentItem> files)
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
                Filter = "Медиафайлы|*.mp4;*.avi;*.mkv;*.mov;*.mp3;*.wav;*.ogg;*.jpg;*.png;*.gif|Все файлы|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var order = 1;

                foreach (var filePath in openFileDialog.FileNames)
                {
                    var fileInfo = new FileInfo(filePath);
                    var extension = Path.GetExtension(filePath).ToLower().TrimStart('.');
                    var mediaType = GetMediaType(extension);

                    selectedFiles.Add(new ContentItem
                    {
                        Name = Path.GetFileNameWithoutExtension(filePath),
                        Type = mediaType,
                        Order = order,
                        FilePath = filePath,
                        Size = fileInfo.Length
                    });

                    order++;
                }

                UpdateUI();
            }
        }

        private static readonly Dictionary<string, string> mediaTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {"mp4", "video"}, {"avi", "video"}, {"mkv", "video"}, {"mov", "video"},
            {"wmv", "video"}, {"flv", "video"}, {"webm", "video"},
            
            {"mp3", "audio"}, {"wav", "audio"}, {"ogg", "audio"}, {"flac", "audio"},
            {"aac", "audio"}, {"wma", "audio"},
            
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
}
