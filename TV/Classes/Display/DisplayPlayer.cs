using System;
using System.Linq;
using TV.Windows;

namespace TV.Classes.Display
{
    public class DisplayPlayer
    {
        private Display targetDisplay;
        private Playlist currentPlaylist;
        private ContentToDisplay currentWindow;

        private int currentItemIndex = 0;

        private System.Windows.Threading.DispatcherTimer timer;
        
        public DisplayPlayer(Display display, Playlist playlist)
        {
            targetDisplay = display;
            currentPlaylist = playlist;

            timer = new System.Windows.Threading.DispatcherTimer();
            timer.Tick += Timer_Tick;
        }

        public async void StartPlayback()
        {
            if (currentPlaylist?.Items == null || !currentPlaylist.Items.Any())
            {
                var connection = new Common.Connection();
                var items = await connection.GetPlaylistItemsAsync(currentPlaylist.Id);

                currentPlaylist.Items = items;
            }

            if (currentPlaylist.Items == null || !currentPlaylist.Items.Any())
            {
                return;
            }

            ShowCurrentItem();
            StartTimerForNextItem();
        }

        public void StopPlayback()
        {
            timer.Stop();
            CloseCurrentWindow();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            PlayNextItem();
        }

        private void PlayNextItem()
        {
            currentItemIndex++;

            if (currentItemIndex >= currentPlaylist.Items.Count)
            {
                currentItemIndex = 0;
                StopPlayback();
                return;
            }

            ShowCurrentItem();
            StartTimerForNextItem();
        }

        private void ShowCurrentItem()
        {
            var currentItem = currentPlaylist.Items[currentItemIndex];
            CloseCurrentWindow();

            currentWindow = new ContentToDisplay(currentItem, targetDisplay);
            currentWindow.Show();
        }

        private void StartTimerForNextItem()
        {
            var currentItem = currentPlaylist.Items[currentItemIndex];
            int duration = GetDurationInSeconds(currentItem);

            timer.Interval = TimeSpan.FromSeconds(duration);
            timer.Start();
        }

        private int GetDurationInSeconds(ContentItem item)
        {
            if (item.Duration > 0)
            {
                return item.Duration;
            }

            switch (item.Type)
            {
                case "image":
                    return 5;
                case "video":
                    return 0;
                default:
                    return 5;
            }
        }

        private void CloseCurrentWindow()
        {
            if (currentWindow != null)
            {
                currentWindow.Close();
                currentWindow = null;
            }
        } 
    }
}
