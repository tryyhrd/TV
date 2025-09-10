using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using TV.Classes;

namespace TV.Pages
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Page
    {
        private ObservableCollection<Display> displays = new ObservableCollection<Display>();
        public Main()
        {
            InitializeComponent();

            contentTypeCombo.SelectedIndex = 1;
        }

        private void DetectDisplays(object sender, RoutedEventArgs e)
        {
            var screens = Screen.AllScreens;

            for (int i = 0; i < screens.Length; i++)
            {
                var display = new Display
                {
                    Id = i,
                    Name = screens[i].DeviceName,
                    Resolution = $"{screens[i].Bounds.Width}x{screens[i].Bounds.Height}",
                    Screen = screens[i],
                    Status = "Обнаружен"
                };

                displays.Add(display);
            }

            displaysGrid.ItemsSource = displays;
        }

        private void TestAllDisplays(object sender, RoutedEventArgs e)
        {

        }

        private void StartAll(object sender, RoutedEventArgs e)
        {

        }

        private void StopAll(object sender, RoutedEventArgs e)
        {
            
        }

        private void StartDisplay(object sender, RoutedEventArgs e)
        {

        }

        private void StopDisplay(object sender, RoutedEventArgs e)
        {

        }

        private void ConfigureDisplay(object sender, RoutedEventArgs e)
        {

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

        private void ContentType_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (contentTypeCombo.SelectedItem == null) return;

            var selectedType = (contentTypeCombo.SelectedItem as ComboBoxItem)?.Content.ToString();

            playlistPanel.Visibility = Visibility.Collapsed;
            mediaFilePanel.Visibility = Visibility.Collapsed;
            webUrlPanel.Visibility = Visibility.Collapsed;

            switch (selectedType)
            {
                case "Плейлист":
                    playlistPanel.Visibility = Visibility.Visible;
                    selectionInfo.Text = "Выберите плейлист и дисплеи для применения";
                    break;

                case "Медиафайл":
                    mediaFilePanel.Visibility = Visibility.Visible;
                    selectionInfo.Text = "Выберите файл и дисплеи для применения";
                    break;

                case "Веб-страница":
                    webUrlPanel.Visibility = Visibility.Visible;
                    selectionInfo.Text = "Введите URL и выберите дисплеи для применения";
                    break;
            }
        }

        private void BrowseMediaFile(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "Видео файлы|*.mp4;*.avi;*.mov;*.wmv;*.mkv|" +
                     "Изображения|*.jpg;*.png;*.bmp;*.gif|" +
                     "Все файлы|*.*",
                Title = "Выберите медиафайл"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                mediaFilePath.Text = openFileDialog.FileName;
            }
        }

        private void ApplyContentToSelected(object sender, RoutedEventArgs e)
        {
            var selectedDisplays = displays.Where(d => d.IsSelected).ToList();

            if (selectedDisplays.Count == 0)
            {
                System.Windows.MessageBox.Show("Выберите хотя бы один дисплей!");
                return;
            }

            var contentTypeItem = contentTypeCombo.SelectedItem as ComboBoxItem;
            string contentType = contentTypeItem?.Content.ToString();

            foreach (var display in selectedDisplays)
            {
                switch (contentType)
                {
                    case "Плейлист":
                        display.CurrentContent = playlistCombo.Text;
                        display.ContentType = "Плейлист";
                        break;

                    case "Медиафайл":
                        display.CurrentContent = mediaFilePath.Text;
                        display.ContentType = "Медиа";
                        break;

                    case "Веб-страница":
                        display.CurrentContent = webUrlTextBox.Text;
                        display.ContentType = "Веб";
                        break;
                }

                display.Status = "Контент назначен";

                Windows.ContentToDisplay contentToDisplay = new Windows.ContentToDisplay(display);
                PositionWindowOnDisplay(contentToDisplay, display);
                contentToDisplay.Show();
            }
        }

        private void PositionWindowOnDisplay(Window window, Display display)
        {
            var screens = Screen.AllScreens;

            if (display.Id >= 0 && display.Id < screens.Length)
            {
                var targetScreen = screens[display.Id];
                SetWindowToScreen(window, targetScreen);
            }
            else if (!string.IsNullOrEmpty(display.Name))
            {
                var targetScreen = screens.FirstOrDefault(s =>
                    s.DeviceName.Equals(display.Name, StringComparison.OrdinalIgnoreCase));

                if (targetScreen != null)
                {
                    SetWindowToScreen(window, targetScreen);
                }
                else
                {
                    SetWindowToScreen(window, Screen.PrimaryScreen);
                }
            }
            else
            {
                SetWindowToScreen(window, Screen.PrimaryScreen);
            }
        }

        private void SetWindowToScreen(Window window, Screen screen)
        {
            window.WindowStartupLocation = WindowStartupLocation.Manual;

            Rectangle workingArea = screen.WorkingArea;

            window.Left = workingArea.Left;
            window.Top = workingArea.Top;
            window.Width = workingArea.Width;
            window.Height = workingArea.Height;

            window.WindowStyle = WindowStyle.None;
            window.WindowState = WindowState.Normal;
            window.ResizeMode = ResizeMode.NoResize;
            window.Topmost = true; 
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
