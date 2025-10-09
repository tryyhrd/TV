using System;
using System.Linq;
using System.Windows.Threading;
using TV.Windows;

namespace TV.Classes.Display
{
    public class DisplayPlayer
    {
        private Playlist _playlist;
        private ContentToDisplay _window;
        private int _currentIndex = 0;
        private DispatcherTimer _timer = new DispatcherTimer();

        public event Action PlaylistEnded;

        public DisplayPlayer(Playlist playlist, ContentToDisplay window)
        {
            _playlist = playlist;
            _window = window;

            _timer.Tick += (s, e) => NextItem();
        }

        public async void Start()
        {
            if (_playlist.Items == null || !_playlist.Items.Any())
            {
                var connection = new Common.Connection();
                _playlist.Items = await connection.GetPlaylistItemsAsync(_playlist.Id);
            }

            ShowCurrentItem();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        public void NextItem()
        {
            Stop();
            _currentIndex++;

            if (_currentIndex >= _playlist.Items.Count)
            {
                PlaylistEnded?.Invoke(); 
                return;
            }
            ShowCurrentItem();
        }

        private void ShowCurrentItem()
        {
            var item = _playlist.Items[_currentIndex];
            _window.PlayContent(item); 

            if (item.Type != "video")
            {
                int duration = item.Duration > 0 ? item.Duration :
                              item.Type == "image" ? 5 : 10;
                _timer.Interval = TimeSpan.FromSeconds(duration);
                _timer.Start();
            }
        }
    }
}
