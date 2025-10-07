using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using TV.Classes;
using TV.Classes.Display;
using TV.Windows;

namespace TV.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {

        private MainViewModel viewModel;
        private Classes.ViewModels.PlaylistsViewModel playlistsViewModel;

        private readonly Dictionary<int, DisplayPlayer> activePlayers = new Dictionary<int, DisplayPlayer>();
        private readonly Dictionary<int, ContentToDisplay> activeWindows = new Dictionary<int, ContentToDisplay>();

        Microsoft.Win32.OpenFileDialog selectedFile;

        private DateTime _lastPlaylistsErrorTime = DateTime.MinValue;
        private DateTime _lastContentErrorTime = DateTime.MinValue;
        public Main()
        {
            InitializeComponent();

            viewModel = new MainViewModel();
            playlistsViewModel = new Classes.ViewModels.PlaylistsViewModel(); 

            DataContext = viewModel;
            displaysGrid.ItemsSource = viewModel.Displays;

            Loaded += (s, e) =>
            {
                DetectDisplays(s, e);
                LoadPlaylists();
                LoadContentToDisplays();
            };

            contentTypeCombo.SelectedIndex = 1;
        }

        private void DetectDisplays(object sender, RoutedEventArgs e)
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
                    Status = "Обнаружен",
                    IsPrimary = screens[i].Primary
                };

                viewModel.Displays.Add(display);
            }

            viewModel.UpdateActiveDisplaysString();
            UpdateSelectionInfo();
        }

        private async void LoadPlaylists()
        {
            try
            {
                var playlists = await playlistsViewModel.LoadPlaylistsAsync();
                playlistCombo.ItemsSource = playlists;

                if (playlists?.Count > 0)
                    playlistCombo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки плейлистов: {ex.Message}");
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
                        display.CurrentContent = content.ContentValue;
                    }
                }
            }
            catch (Exception ex) 
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки контента для плейлистов: {ex.Message}");
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
                string contentType = "";
                string displayName = "";
                string contentValue = "";

                switch (contentTypeCombo.SelectedIndex)
                {
                    case 0:
                        var selectedPlaylist = playlistCombo.SelectedItem as Playlist;

                        if (selectedPlaylist == null)
                        {
                            ShowErrorMessage("Плейлист не выбран!");
                            return;
                        }

                        contentType = "Плейлист";
                        displayName = selectedPlaylist.Name;
                        contentValue = selectedPlaylist.Id.ToString();

                        foreach (var display in selectedDisplays)
                        {
                            var displayContent = new DisplayContent()
                            {
                                DisplayId = display.Id,
                                ContentType = contentType,
                                ContentValue = selectedPlaylist.Id.ToString(),
                                DisplayName = selectedPlaylist.Name,
                                Playlist = selectedPlaylist,
                                PlaylistId = selectedPlaylist.Id,
                            };
                            await displayContent.SaveToDatabaseAsync();

                            display.ContentType = contentType;
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

                        var newContentItem = new ContentItem
                        {
                            Name = Path.GetFileNameWithoutExtension(selectedFile.FileName),
                            Type = mediaType,
                            Order = 1,
                            FilePath = selectedFile.FileName,
                            Size = fileInfo.Length
                        };

                        contentType = "Медиафайл";
                        displayName = Path.GetFileName(selectedFile.FileName);

                        foreach (var display in selectedDisplays)
                        {
                            var displayContent = new DisplayContent()
                            {
                                DisplayId = display.Id,
                                ContentType = contentType,
                                ContentValue = selectedFile.FileName,
                                DisplayName = selectedFile.SafeFileName,
                                Playlist = null,
                                PlaylistId = null
                            };
                            await displayContent.SaveToDatabaseAsync();

                            display.ContentType = contentType;
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

                        contentType = "Веб-страница";
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
                                ContentType = contentType,
                                ContentValue = contentValue,
                                DisplayName = displayName,
                                PlaylistId = null
                            };

                            await displayContent.SaveToDatabaseAsync();

                            display.ContentType = contentType;
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
            try
            {
                StopDisplayContent(display.Id);

                string contentType = displayContent.ContentType;
                string contentValue = displayContent.ContentValue;

                display.ContentType = contentType;
                display.CurrentContent = displayContent.DisplayName;

                switch (contentType)
                {
                    case "Плейлист":

                        if (displayContent.Playlist != null)
                        {
                            var player = new DisplayPlayer(display, displayContent.Playlist);
                            activePlayers[display.Id] = player;
                            player.StartPlayback();

                            display.Status = "Воспроизведение плейлиста";
                        }

                        else display.Status = "Плейлист не обнаружен";

                        break;

                    case "Медиафайл":
                        string mediaPath = displayContent.ContentValue ?? contentValue;

                        if (File.Exists(mediaPath))
                        {
                            var mediaContentItem = new ContentItem
                            {
                                Name = Path.GetFileNameWithoutExtension(mediaPath),
                                FilePath = mediaPath,
                                Type = GetMediaType(Path.GetExtension(mediaPath))
                                
                            };

                            System.Windows.Forms.MessageBox.Show(mediaContentItem.Type.ToString());

                            var mediaWindow = new ContentToDisplay(mediaContentItem, display);
                            activeWindows[display.Id] = mediaWindow;
                            PositionWindowOnDisplay(mediaWindow, display);
                            mediaWindow.Show();
                            display.Status = "Воспроизведение медиа";
                        }
                        else
                        {
                            display.Status = "Ошибка: файл не найден";
                        }
                        break;

                    case "Веб-страница":
                        string webUrl = displayContent.ContentValue ?? contentValue;
                        var webContentItem = new ContentItem
                        {
                            Name = "Веб-контент",
                            FilePath = webUrl,
                            Type = "web"
                        };

                        var webWindow = new ContentToDisplay(webContentItem, display);
                        activeWindows[display.Id] = webWindow;
                        PositionWindowOnDisplay(webWindow, display);
                        webWindow.Show();
                        display.Status = "Открыта веб-страница";
                        break;

                    default:
                        display.Status = "Неизвестный тип контента";
                        break;
                }

                displaysGrid.Items.Refresh();
            }
            catch (Exception ex)
            {
                display.Status = "Ошибка запуска";
                ShowErrorMessage($"Ошибка запуска контента на дисплее {display.Name}: {ex.Message}");
                displaysGrid.Items.Refresh();
            }
        }

        private void StopDisplayContent(int displayId)
        {
            if (activePlayers.ContainsKey(displayId))
            {
                var player = activePlayers[displayId];
                player.StopPlayback();
                activePlayers.Remove(displayId);
            }

            if (activeWindows.ContainsKey(displayId))
            {
                var window = activeWindows[displayId];
                window.Close();
                activeWindows.Remove(displayId);
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

        private void PositionWindowOnDisplay(Window window, Display display)
        {
            var targetScreen = FindTargetScreen(display);
            SetWindowToScreen(window, targetScreen);
        }

        private Screen FindTargetScreen(Display display)
        {

            var screens = Screen.AllScreens;

            if (display.Id >= 0 && display.Id < screens.Length)
                return screens[display.Id];

            if (!string.IsNullOrEmpty(display.Name))
            {
                var byName = screens.FirstOrDefault(s =>
                    s.DeviceName.Equals(display.Name, StringComparison.OrdinalIgnoreCase));
                if (byName != null) return byName;
            }

            return Screen.PrimaryScreen ?? screens.First();
        }

        private void SetWindowToScreen(Window window, Screen screen)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            var workingArea = screen.WorkingArea;

            window.Left = workingArea.Left;
            window.Top = workingArea.Top;
            window.Width = workingArea.Width;
            window.Height = workingArea.Height;

            window.WindowStyle = WindowStyle.None;
            window.WindowState = WindowState.Normal;
            window.ResizeMode = ResizeMode.NoResize;
            window.Topmost = true;
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

                StopDisplayContent(displayId);

                var display = viewModel.Displays.FirstOrDefault(d => d.Id == displayId);

                if (display != null)
                    display.Status = "Остановлен";
                
            }
        }
    }
}
