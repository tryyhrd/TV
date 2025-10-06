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

        private readonly Dictionary<int, Classes.Display.DisplayPlayer> activePlayers = new Dictionary<int, Classes.Display.DisplayPlayer>();
        private readonly Dictionary<int, ContentToDisplay> activeWindows = new Dictionary<int, ContentToDisplay>();

        Microsoft.Win32.OpenFileDialog selectedFile;
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
                var display = new Classes.Display.Display()
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
            foreach (var display in viewModel.Displays)
            {
                var content = await Classes.Display.DisplayContent.LoadByDisplayIdAsync(display.Id);

                if (content != null)
                {
                    display.ContentType = content.ContentType;
                    display.CurrentContent = content.ContentValue;
                }
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
                            var displayContent = new Classes.Display.DisplayContent()
                            {
                                DisplayId = display.Id,
                                ContentType = contentType,
                                ContentValue = selectedPlaylist.Id.ToString(),
                                DisplayName = selectedPlaylist.Name,
                                Playlist = selectedPlaylist,
                                PlaylistId = selectedPlaylist.Id,
                            };

                            await displayContent.SaveToDatabaseAsync();
                        }

                        break;

                    case 1:

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
                        }
                        break;
                }

                foreach (var display in selectedDisplays)
                {
                    display.ContentType = contentType;
                    display.CurrentContent = displayName;

                    if (contentType == "Плейлист")
                    {
                        display.Status = "Плейлист установлен";
                    }
                    else if (contentType == "Медиафайл")
                    {
                        display.Status = "Контент установлен";
                    }
                    else if (contentType == "Веб-страница")
                    {
                        display.Status = "Веб-страница установлена";
                    }
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

        private void ApplyContentToDisplay(Classes.Display.Display display, Classes.Display.DisplayContent displayContent)
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

                        //var playlists = await new Classes.ViewModels.PlaylistsViewModel().LoadPlaylistsAsync();

                        if (displayContent.Playlist != null)
                        {
                            var player = new Classes.Display.DisplayPlayer(display, displayContent.Playlist);
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

        private string GetCurrentContentType()
        {
            switch (contentTypeCombo.SelectedIndex)
            {
                case 0:
                    return "Плейлист";
                case 1:
                    return "Медиафайл";
                case 2:
                    return "Веб-страница";
                default:
                    return "Неизвестно";
            }
        }

        private string GetCurrentContentValue()
        {
            switch (contentTypeCombo.SelectedIndex)
            {
                case 0:
                    var playlist = playlistCombo.SelectedItem as Playlist;
                    return playlist?.Name;
                case 1:
                    return mediaFilePath.Text;
                case 2:
                    return webUrlTextBox.Text;
                default:
                    return null;
            }
        }

        private string GetContentDisplayName(string contentValue)
        {
            if (string.IsNullOrEmpty(contentValue))
                return "Не указан";

            switch (contentTypeCombo.SelectedIndex)
            {
                case 0:
                    var playlist = playlistCombo.SelectedItem as Playlist;
                    return playlist?.Name ?? "Плейлист";
                case 1:
                    try
                    {
                        return Path.GetFileName(contentValue);
                    }
                    catch
                    {
                        return "Медиафайл";
                    }
                case 2:
                    try
                    {
                        var uri = new Uri(contentValue);
                        return uri.Host ?? "Веб-страница";
                    }
                    catch
                    {
                        return "Веб-страница";
                    }
                default:
                    return contentValue;
            }
        }

        private ContentItem CreateContentItem(string contentType, string contentValue)
        {
            string name;
            string type;

            if (contentType == "Веб-страница")
            {
                name = "Веб-контент";
                type = "web";
            }
            else if (contentType == "Плейлист")
            {
                name = "Плейлист";
                type = "playlist";
            }
            else 
            {
                try
                {
                    name = Path.GetFileNameWithoutExtension(contentValue);
                }
                catch
                {
                    name = "Медиафайл";
                }
                type = GetMediaType(Path.GetExtension(contentValue));
            }

            return new ContentItem
            {
                Name = name,
                FilePath = contentValue,
                Type = type
            };
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
            switch (extension)
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

        private void PositionWindowOnDisplay(Window window, Classes.Display.Display display)
        {
            var targetScreen = FindTargetScreen(display);
            SetWindowToScreen(window, targetScreen);
        }

        private Screen FindTargetScreen(Classes.Display.Display display)
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
                var displayContent = await Classes.Display.DisplayContent.LoadByDisplayIdAsync(display.Id);

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

        private void StartDisplay(object sender, RoutedEventArgs e)
        {
            //if (sender is System.Windows.Controls.Button button && button.tag != null)
            //{
            //    int displayid = (int)button.tag;
            //    var display = viewmodel.displays.firstordefault(d => d.id == displayid);
            //    if (display != null)
            //    {
            //        applycontenttodisplay(display);
            //    }
            //}
        }

        private void StopDisplay(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag != null)
            {
                int displayId = (int)button.Tag;
                StopDisplayContent(displayId);

                var display = viewModel.Displays.FirstOrDefault(d => d.Id == displayId);
                if (display != null)
                {
                    display.Status = "Остановлен";
                    display.CurrentContent = "Нет контента";
                }
            }
        }

        private void RefreshDataGrid()
        {
            displaysGrid.Items.Refresh();
        }

        private void StartPlaylistOnDisplay(object sender, RoutedEventArgs e)
        {

        }

        private void StartMediaOnDisplay(object sender, RoutedEventArgs e)
        {

        }

        private void StopDisplayPlayback(object sender, RoutedEventArgs e)
        {

        }

        private void PreviewDisplay(object sender, RoutedEventArgs e)
        {

        }

        private void SelectAllDisplays(object sender, RoutedEventArgs e)
        {

        }

        private void DeselectAllDisplays(object sender, RoutedEventArgs e)
        {

        }

        private void StartPlaylistOnSelected(object sender, RoutedEventArgs e)
        {

        }

        private void StartMediaOnSelected(object sender, RoutedEventArgs e)
        {

        }

        private void StartWebOnSelected(object sender, RoutedEventArgs e)
        {

        }

        private void StopSelectedDisplays(object sender, RoutedEventArgs e)
        {

        }

        private void ConfigureSelectedDisplays(object sender, RoutedEventArgs e)
        {

        }
    }
}
