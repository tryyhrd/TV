using System.Threading.Tasks;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Wpf;
using TV.Classes;
using TV.Classes.Display;
using Microsoft.Web.WebView2.Core;
using System.IO;

namespace TV.Windows
{
    /// <summary>
    /// Логика взаимодействия для ContentToDisplay.xaml
    /// </summary>
    public partial class ContentToDisplay : Window
    {
        private MediaElement _mediaElement;
        private Image _imageElement;
        private WebView2 _webView;
        private DispatcherTimer _contentTimer;

        private Playlist _currentPlaylist;
        private PlaylistItem _currentItem;
        public event Action PlaylistEnded;

        private int _currentIndex = 0;

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

            var targetScreen = FindTargetScreen(display);
            SetWindowToScreen(this, targetScreen);

            Closing += (s, e) => StopPlayback();

            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape || e.Key == Key.Q)
                {
                    Close();
                }
                else if (e.Key == Key.Space)
                {
                    if (_mediaElement.Visibility == Visibility.Visible)
                    {
                        if (_mediaElement.CanPause && _mediaElement.Position > TimeSpan.Zero)
                        {
                            _mediaElement.Pause();
                        }
                        else
                        {
                            _mediaElement.Play();
                        }
                    }
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
            var grid = new Grid();
            grid.Background = Brushes.Black;

            _mediaElement = new MediaElement
            {
                LoadedBehavior = MediaState.Manual,
                UnloadedBehavior = MediaState.Stop,
                Visibility = Visibility.Collapsed
            };

            _mediaElement.MediaEnded += (s, e) => NextItem();
            _mediaElement.MediaFailed += (s, e) => NextItem();

            _imageElement = new Image
            {
                Stretch = Stretch.Uniform,
                Visibility = Visibility.Collapsed
            };

            _webView = new WebView2
            {
                Visibility = Visibility.Collapsed
            };

            grid.Children.Add(_mediaElement);
            grid.Children.Add(_imageElement);
            grid.Children.Add(_webView);

            Content = grid;

            _contentTimer = new DispatcherTimer();
            _contentTimer.Tick += (s, e) => NextItem();
        }

        private void PlayCurrentItem()
        {
            if (_currentPlaylist?.Items == null || _currentIndex >= _currentPlaylist.Items.Count)
            {
                PlaylistEnded?.Invoke();
                return;
            }

            _currentItem = _currentPlaylist.Items[_currentIndex];
            PlayContent(_currentItem);
        }

        public void PlayContent(PlaylistItem content)
        {
            StopPlayback();

            _mediaElement.Visibility = Visibility.Collapsed;
            _imageElement.Visibility = Visibility.Collapsed;
            _webView.Visibility = Visibility.Collapsed;

            try
            {
                switch (content.Type.ToLower())
                {
                    case "video":
                        PlayVideoContent(content);
                        break;

                    case "image":
                        PlayImageContent(content);
                        break;

                    case "web":
                        PlayWebContent(content);
                        break;
                }
            }
            catch { }
        }

        private void NextItem()
        {
            _contentTimer.Stop();
            _currentIndex++;

            if (_currentIndex >= _currentPlaylist?.Items.Count)
            {
                PlaylistEnded?.Invoke();
                return;
            }

            PlayCurrentItem();
        }

        private void PlayVideoContent(PlaylistItem content)
        {
            try
            {
                _mediaElement.Source = new Uri(content.FilePath);
                _mediaElement.Visibility = Visibility.Visible;
                _mediaElement.Play();
            }
            catch
            {
                NextItem();
            }
        }

        private void PlayImageContent(PlaylistItem content)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(content.FilePath);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                _imageElement.Source = bitmap;
                _imageElement.Visibility = Visibility.Visible;

                int duration = content.Duration > 0 ? content.Duration : 5;
                _contentTimer.Interval = TimeSpan.FromSeconds(duration);
                _contentTimer.Start();
            }
            catch
            {
                NextItem();
            }
        }

        private async void PlayWebContent(PlaylistItem content)
        {
            try
            {
                string url = NormalizeWebUrl(content.FilePath);

                await InitializeWebView2Safe();

                _webView.Visibility = Visibility.Visible;
                _webView.Source = new Uri(url);

                if (content.Duration > 0)
                {
                    _contentTimer.Interval = TimeSpan.FromSeconds(content.Duration);
                    _contentTimer.Start();
                }
            }
            catch { }
        }


        private async Task InitializeWebView2Safe()
        {
            if (_webView.CoreWebView2 == null)
            {
                string tempCachePath = Path.Combine(Path.GetTempPath(), "TV_WebView2_Cache");

                var environment = await CoreWebView2Environment.CreateAsync(
                    browserExecutableFolder: null,
                    userDataFolder: tempCachePath,
                    options: new CoreWebView2EnvironmentOptions()
                );

                await _webView.EnsureCoreWebView2Async(environment);

                if (_webView.CoreWebView2 != null)
                {
                    _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                    _webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

                    await _webView.CoreWebView2.Profile.ClearBrowsingDataAsync();
                }
            }
        }

        private string NormalizeWebUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "about:blank";

            if (!url.StartsWith("http://") && !url.StartsWith("https://") && !url.StartsWith("file://"))
            {
                return "https://" + url;
            }

            return url;
        }

        public async void StartPlaylist(Playlist playlist)
        {
            _currentPlaylist = playlist;
            _currentIndex = 0;

            if (_currentPlaylist.Items == null || !_currentPlaylist.Items.Any())
            {
                var connection = new Classes.Common.Connection();
                _currentPlaylist.Items = await connection.GetPlaylistItemsAsync(_currentPlaylist.Id);
            }

            PlayCurrentItem();
        }

        public void StopPlayback()
        {
            _contentTimer?.Stop();
            _mediaElement?.Stop();
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
