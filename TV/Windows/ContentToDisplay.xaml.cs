using System.IO;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TV.Classes;
using TV.Classes.Display;
using System.Windows.Input;
using System.Windows.Forms;
using System.Linq;

namespace TV.Windows
{
    /// <summary>
    /// Логика взаимодействия для ContentToDisplay.xaml
    /// </summary>
    public partial class ContentToDisplay : Window
    {
        private MediaElement _mediaElement;
        private Image _imageElement;
        private System.Windows.Controls.WebBrowser _webBrowser;
        private DisplayPlayer _currentPlayer;
        public ContentToDisplay(Display display)
        {
            InitializeComponent();

            CreateMediaElements();
            SetupWindow(display); 
        }

        private void SetupWindow(Display display)
        {
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            //Topmost = true;

            var targetScreen = FindTargetScreen(display);
            SetWindowToScreen(this, targetScreen);

            Closing += (s, e) => StopPlayback();

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape || e.Key == Key.Q)
                {
                    Close();
                }
            };
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

        private void CreateMediaElements()
        {
            _mediaElement = new MediaElement
            {
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop
            };

            _imageElement = new Image
            {
                Stretch = Stretch.Uniform
            };

            _webBrowser = new System.Windows.Controls.WebBrowser();

            _mediaElement.MediaEnded += (s, e) => _currentPlayer?.NextItem();

            var grid = new Grid();

            grid.Children.Add(_mediaElement);
            grid.Children.Add(_imageElement);
            grid.Children.Add(_webBrowser);

            _mediaElement.Visibility = Visibility.Collapsed;
            _imageElement.Visibility = Visibility.Collapsed;
            _webBrowser.Visibility = Visibility.Collapsed;

            Content = grid;
        }

        public void PlayContent(ContentItem content)
        {
            StopPlayback();

            _mediaElement.Visibility = Visibility.Collapsed;
            _imageElement.Visibility = Visibility.Collapsed;
            _webBrowser.Visibility = Visibility.Collapsed;

            switch (content.Type)
            {
                case "video":
                    _mediaElement.Source = new Uri(content.FilePath);
                    _mediaElement.Visibility = Visibility.Visible;
                    _mediaElement.Play();
                    break;
                case "image":
                    _imageElement.Source = new BitmapImage(new Uri(content.FilePath));
                    _imageElement.Visibility = Visibility.Visible;
                    break;
                case "web":
                    _webBrowser.Navigate(content.FilePath);
                    _webBrowser.Visibility = Visibility.Visible;
                    break;
            }
        }

        public void StartPlaylist(Playlist playlist)
        {
            _currentPlayer = new DisplayPlayer(playlist, this);

            _currentPlayer.PlaylistEnded += () =>
            {
                Dispatcher.Invoke(Close);
            };

            _currentPlayer.Start();
        }

        public void StopPlayback()
        {
            _currentPlayer?.Stop();
            _currentPlayer = null;

            _mediaElement.Stop();
            _mediaElement.Source = null;
            _imageElement.Source = null;
        }

        public void ForceClose()
        {
            StopPlayback();
            Close();
        }
    }
}
    