using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using TV.Classes;
using TV.Classes.Display;
using TV.Classes.ViewModels;
using TV.Windows;

namespace TV.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {

        private DisplayViewModel viewModel;
        private PlaylistsViewModel playlistsViewModel = new PlaylistsViewModel();
        private List<Playlist> _playlists;

        private Dictionary<int, ContentToDisplay> _displayWindows = new Dictionary<int, ContentToDisplay>();

        Microsoft.Win32.OpenFileDialog selectedFile;
        public Main()
        {
            InitializeComponent();

            viewModel = new DisplayViewModel();

            DataContext = viewModel;
            displaysGrid.ItemsSource = viewModel.Displays;

            Loaded += async (s, e) =>
            {
                DetectDisplays(s, e);
                await LoadPlaylists();
                LoadContentToDisplays();
            };

            contentTypeCombo.SelectedIndex = 1;
        }

        private async void DetectDisplays(object sender, RoutedEventArgs e)
        {
            await DetectDisplays();
        }

        private async Task DetectDisplays()
        {
            viewModel.Displays.Clear();
            
            var screens = Screen.AllScreens;

            for (int i = 0; i < screens.Length; i++)
            {
                var display = new Display()
                {
                    Id = i,
                    Name = screens[i].DeviceName,
                    Resolution = $"{screens[i].Bounds.Width}x{screens[i].Bounds.Height}",
                    Screen = screens[i],
                    IsPrimary = screens[i].Primary
                };

                var content = await DisplayContent.LoadByDisplayIdAsync(display.Id);

                if (content != null)
                {
                    switch (content.ContentType)
                    {
                        case "Плейлист":
                            display.Status = "Плейлист установлен";
                            break;
                        case "Медиафайл":
                            display.Status = "Медиа контент установлен";
                            break;
                        case "Веб":
                            display.Status = "Веб-контент установлен";
                            break;
                        case null:
                            display.Status = "Дисплей неактивен";
                            break;
                    }
                }

                viewModel.Displays.Add(display);
            }

            viewModel.UpdateActiveDisplaysString();
            UpdateSelectionInfo();
        }

        private async Task LoadPlaylists()
        {
            try
            {
                _playlists = await playlistsViewModel.LoadPlaylistsAsync();
                playlistCombo.ItemsSource = _playlists;

                if (_playlists?.Count > 0)
                    playlistCombo.SelectedIndex = 0;
            }
            catch
            {
                return;
            }
        }

        private async void LoadContentToDisplays()
        {
            try
            {
                foreach (var display in viewModel.Displays)
                {
                    var content = await DisplayContent.LoadByDisplayIdAsync(display.Id);

                    if (content != null)
                    {
                        display.ContentType = content.ContentType;

                        switch (display.ContentType)
                        {
                            case "Плейлист":
                                display.CurrentContent = _playlists.First(x => x.Id == int.Parse(content.ContentValue)).Name;
                                break;
                            case "Медиафайл":
                                display.CurrentContent = Path.GetFileName(content.ContentValue);
                                break;
                            case "Веб":
                                display.CurrentContent = _playlists.First(x => x.Id == int.Parse(content.ContentValue)).Name;
                                break;
                            default:
                                display.CurrentContent = "Нет";
                                break;
                        }
                    }
                }
            }
            catch 
            {
                return;
            }          
        }

        private void ContentType_Changed(object sender, SelectionChangedEventArgs e)
        {
            playlistPanel.Visibility = Visibility.Collapsed;
            mediaFilePanel.Visibility = Visibility.Collapsed;
            webUrlPanel.Visibility = Visibility.Collapsed;

            switch (contentTypeCombo.SelectedIndex)
            {
                case 0:
                    playlistPanel.Visibility = Visibility.Visible;
                    break;
                case 1:
                    mediaFilePanel.Visibility = Visibility.Visible;
                    mediaFilePath.Text = "";
                    break;
                case 2:
                    webUrlPanel.Visibility = Visibility.Visible;
                    webUrlTextBox.Text = "https://";
                    break;
            }

            UpdateSelectionInfo();
        }

        private void BrowseMediaFile(object sender, RoutedEventArgs e)
        {
            selectedFile = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Все поддерживаемые форматы|*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp;|" +
                "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.webp;*.tiff;*.svg|" +
                "Видео файлы|*.mp4;*.avi;*.mov;*.wmv;*.mkv;*.flv;*.webm;*.m4v|" +               
                 "Все файлы|*.*",
                Title = "Выберите медиафайл",
                Multiselect = false
            };

            if (selectedFile.ShowDialog() == true)
            {
                mediaFilePath.Text = selectedFile.FileName;

                var extension = Path.GetExtension(selectedFile.FileName).ToLower();
                var fileType = GetMediaType(extension);

                switch (fileType)
                {
                    case "video":
                        displaysStatus.Text = $"Выбрано видео: {Path.GetFileName(selectedFile.FileName)}";
                        break;
                    case "audio":
                        displaysStatus.Text = $"Выбрано аудио: {Path.GetFileName(selectedFile.FileName)}";
                        break;
                    case "image":
                        displaysStatus.Text = $"Выбрано изображение: {Path.GetFileName(selectedFile.FileName)}";
                        break;
                    default:
                        displaysStatus.Text = $"Выбран файл: {Path.GetFileName(selectedFile.FileName)}";
                        break;
                }
            }
        }

        private async void ApplyContentToSelected(object sender, RoutedEventArgs e)
        {
            var selectedDisplays = viewModel.Displays.Where(d => d.IsSelected).ToList();

            if (selectedDisplays.Count == 0)
            {
                ShowErrorMessage("Выберите хотя бы один дисплей!");
                return;
            }

            try
            {
                string contentMode = "";
                string contentType = "";
                string displayName = "";
                string contentValue = "";
                int? playlistId = null;

                switch (contentTypeCombo.SelectedIndex)
                {
                    case 0:
                        var selectedPlaylist = playlistCombo.SelectedItem as Playlist;

                        if (selectedPlaylist == null)
                        {
                            ShowErrorMessage("Плейлист не выбран!");
                            return;
                        }

                        contentMode = "PLAYLIST";
                        contentType = null; 
                        displayName = selectedPlaylist.Name;
                        contentValue = null;
                        playlistId = selectedPlaylist.Id;

                        foreach (var display in selectedDisplays)
                        {
                            var displayContent = new DisplayContent()
                            {
                                DisplayId = display.Id,
                                ContentMode = contentMode,
                                ContentType = contentType,
                                ContentValue = contentValue,
                                Name = displayName,
                                PlaylistId = playlistId,
                                ScheduleId = null,
                                DisplayDuration = null, 
                                StartDateTime = null,
                                EndDateTime = null,
                                IsLoop = true,
                                Priority = 1,
                                IsActive = true
                            };
                            await displayContent.SaveToDatabaseAsync();

                            display.ContentType = "Плейлист";
                            display.CurrentContent = displayName;
                            display.Status = "Плейлист установлен";
                        }
                        break;

                    case 1:
                        if (selectedFile == null)
                        {
                            ShowErrorMessage($"Файл не выбран");
                            return;
                        }
                        if (!File.Exists(selectedFile.FileName))
                        {
                            ShowErrorMessage($"Файл не существует: {selectedFile.SafeFileName}");
                            return;
                        }

                        var fileInfo = new FileInfo(selectedFile.FileName);
                        var extension = Path.GetExtension(selectedFile.FileName).ToLower().TrimStart('.');
                        var mediaType = GetMediaType(extension);

                        contentMode = "SIMPLE";
                        contentType = mediaType.ToLower();
                        displayName = Path.GetFileName(selectedFile.FileName);
                        contentValue = selectedFile.FileName;
                        playlistId = null;

                        foreach (var display in selectedDisplays)
                        {
                            var displayContent = new DisplayContent()
                            {
                                DisplayId = display.Id,
                                ContentMode = contentMode,
                                ContentType = contentType,
                                ContentValue = contentValue,
                                Name = displayName,
                                PlaylistId = null,
                                ScheduleId = null,
                                DisplayDuration = null,
                                StartDateTime = null,
                                EndDateTime = null,
                                IsLoop = true,
                                Priority = 1,
                                IsActive = true
                            };
                            await displayContent.SaveToDatabaseAsync();

                            display.CurrentContent = displayName;
                            display.Status = "Контент установлен";
                        }
                        break;

                    case 2:
                        if (string.IsNullOrEmpty(webUrlTextBox.Text) || webUrlTextBox.Text == "https://")
                        {
                            ShowErrorMessage("URL не указан!");
                            return;
                        }

                        contentMode = "SIMPLE";
                        contentType = "web";
                        contentValue = webUrlTextBox.Text;

                        try
                        {
                            var uri = new Uri(webUrlTextBox.Text);
                            displayName = uri.Host;
                        }
                        catch
                        {
                            displayName = "Веб-страница";
                        }

                        foreach (var display in selectedDisplays)
                        {
                            var displayContent = new DisplayContent()
                            {
                                DisplayId = display.Id,
                                ContentMode = contentMode,
                                ContentType = contentType,
                                ContentValue = contentValue,
                                Name = displayName,
                                PlaylistId = null,
                                ScheduleId = null,
                                DisplayDuration = 300, 
                                StartDateTime = null,
                                EndDateTime = null,
                                IsLoop = true,
                                Priority = 1,
                                IsActive = true
                            };

                            await displayContent.SaveToDatabaseAsync();

                            display.ContentType = "Веб-страница";
                            display.CurrentContent = displayName;
                            display.Status = "Веб-страница установлена";
                        }
                        break;
                }

                UpdateSelectionInfo();
                displaysGrid.Items.Refresh();

                ShowSuccessMessage($"Контент применен к {selectedDisplays.Count} дисплеям");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Ошибка применения контента: {ex.Message}");
            }
        }

        private void ApplyContentToDisplay(Display display, DisplayContent displayContent)
        {
            StopDisplayContent(display.Id);

            var window = GetOrCreateDisplayWindow(display);

            window.PlaylistEnded += () =>
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    window.ForceClose();
                }));
            };

            switch (displayContent.ContentMode)
            {
                case "SIMPLE":
                    ApplySimpleContent(window, display, displayContent);
                    break;
                case "PLAYLIST":
                    window.StartPlaylist(displayContent.Playlist);
                    display.Status = "Воспроизведение плейлиста";
                    break;
                case "SCHEDULE":
                    break;
            }
        }

        private void ApplySimpleContent(ContentToDisplay window, Display display, DisplayContent displayContent)
        {
            var contentType = displayContent.ContentType?.ToLower();
            var contentValue = displayContent.ContentValue;

            if (string.IsNullOrEmpty(contentValue))
            {
                display.Status = "Ошибка: контент не указан";
                return;
            }

            var playlistItem = new PlaylistItem
            {
                FilePath = contentValue,
                Duration = displayContent.DisplayDuration ?? 0
            };

            switch (contentType)
            {
                case "video":
                case "image":
                    playlistItem.Type = contentType;
                    window.PlayContent(playlistItem);
                    display.Status = $"Воспроизведение {(contentType)}";
                    break;

                case "web":
                    playlistItem.Type = "web";
                    window.PlayContent(playlistItem);
                    display.Status = "Открыта веб-страница";
                    break;

                case "audio":
                    playlistItem.Type = "video"; // Аудио воспроизводим как видео
                    window.PlayContent(playlistItem);
                    display.Status = "Воспроизведение аудио";
                    break;

                default:
                    // Автоопределение типа по расширению файла
                    var detectedType = GetMediaType(Path.GetExtension(contentValue));
                    playlistItem.Type = detectedType;
                    window.PlayContent(playlistItem);
                    display.Status = $"Воспроизведение {detectedType}";
                    break;
            }
        }

        private ContentToDisplay GetOrCreateDisplayWindow(Display display)
        {
            if (_displayWindows.ContainsKey(display.Id) && _displayWindows[display.Id] != null)
            {
                return _displayWindows[display.Id];
            }

            var window = new ContentToDisplay(display);
            _displayWindows[display.Id] = window;

            window.Closed += (s, e) => _displayWindows.Remove(display.Id);

            window.Show();
            return window;
        }

        private void StopDisplayContent(int displayId)
        {
            if (_displayWindows.ContainsKey(displayId))
            {
                _displayWindows[displayId].StopPlayback();
                _displayWindows[displayId].Close();
                _displayWindows.Remove(displayId);
            }
        }

        private string GetMediaType(string extension)
        {
            switch (extension.Replace('.', ' ').Trim())
            {
                case "mp4":
                case "avi":
                case "mkv":
                case "mov":
                case "wmv":
                case "flv":
                case "webm":
                    return "video";

                case "jpg":
                case "jpeg":
                case "png":
                case "gif":
                case "bmp":
                case "tiff":
                case "webp":
                    return "image";

                default:
                    return "unknown";
            }
        }

        private void UpdateSelectionInfo()
        {
            var selectedCount = viewModel.Displays.Count(d => d.IsSelected);
            selectionInfo.Text = selectedCount > 0
                ? $"Выбрано дисплеев: {selectedCount}"
                : "Выберите дисплеи в таблице ниже";
        }

        private void ShowErrorMessage(string message)
        {
            System.Windows.MessageBox.Show(message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ShowSuccessMessage(string message)
        {
            displaysStatus.Text = message;
            var timer = new System.Windows.Threading.DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);
            timer.Tick += (s, e) =>
            {
                displaysStatus.Text = "Готово";
                timer.Stop();
            };
            timer.Start();
        }

        private async void StartAll(object sender, RoutedEventArgs e)
        {
            var selectedDisplays = viewModel.Displays.Where(d => d.IsSelected).ToList();

            if (selectedDisplays.Count == 0)
            {
                ShowErrorMessage("Выберите дисплеи для запуска!");
                return;
            }

            foreach (var display in selectedDisplays)
            {
                var displayContent = await DisplayContent.LoadByDisplayIdAsync(display.Id);

                ApplyContentToDisplay(display, displayContent);
            }

            UpdateSelectionInfo();
        }

        private void StopAll(object sender, RoutedEventArgs e)
        {
            var selectedDisplays = viewModel.Displays.Where(d => d.IsSelected).ToList();
            if (selectedDisplays.Count == 0)
            {
                ShowErrorMessage("Выберите дисплеи для остановки!");
                return;
            }

            foreach (var display in selectedDisplays)
            {
                StopDisplayContent(display.Id);
                display.Status = "Остановлен";
                display.CurrentContent = "Нет контента";
            }

            UpdateSelectionInfo();
        }

        private async void StartDisplay(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag != null)
            {
                int displayid = (int)button.Tag;
                var display = viewModel.Displays.FirstOrDefault(d => d.Id == displayid);
                if (display != null)
                {
                    var displayContent = await DisplayContent.LoadByDisplayIdAsync(display.Id);

                    ApplyContentToDisplay(display, displayContent);
                }
            }
        }

        private void StopDisplay(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag != null)
            {
                int displayId = (int)button.Tag;
                var display = viewModel.Displays.FirstOrDefault(d => d.Id == displayId);

                if (display == null)
                    return;

                display.Status = "Остановлен";

                StopDisplayContent(displayId);  
            }
        }
    }
}
