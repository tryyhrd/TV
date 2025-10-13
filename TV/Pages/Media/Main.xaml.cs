using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TV.Classes;
using TV.Classes.ViewModels;

namespace TV.Pages.Media
{
    /// <summary>
    /// Логика взаимодействия для Media.xaml
    /// </summary>
    public partial class Main : Page
    {
        private PlaylistsViewModel viewModel;
        private Playlist selectedPlaylist;
        public Main()
        {
            InitializeComponent();

            viewModel = new PlaylistsViewModel();
            DataContext = viewModel;

            LoadPlaylists();
        }

        private async void LoadPlaylists()
        {
            try
            {
                await viewModel.LoadPlaylistsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки плейлистов: {ex.Message}");
            }
        }

        private void Playlist_Selected(object sender, SelectionChangedEventArgs e)
        {
            selectedPlaylist = playlistsListView.SelectedItem as Playlist;

            if ( selectedPlaylist == null )
            {
                return;
            }

            leftColumn.Width = new GridLength(1, GridUnitType.Auto);
            rightColumn.Width = new GridLength(1, GridUnitType.Star);
            gridSpliter.Width = new GridLength(5, GridUnitType.Pixel);

            splitterColumn.Visibility = Visibility.Visible;
            rightContentPanel.Visibility = Visibility.Visible;

            currentPlaylistName.Text = selectedPlaylist.Name;
            currentPlaylistInfo.Text = $"{selectedPlaylist.ItemCount} элементов • {selectedPlaylist.Duration}";

            if (!string.IsNullOrEmpty(selectedPlaylist.Description))
            {
                playlistDescriptionText.Text = selectedPlaylist.Description;
                playlistDescriptionText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); 
            }
            else
            {
                playlistDescriptionText.Text = "Описание не указано";
                playlistDescriptionText.Foreground = new SolidColorBrush(Color.FromRgb(156, 163, 175));
            }

            playlistContentGrid.ItemsSource = selectedPlaylist.Items;
        }

        private void CreatePlaylist(object sender, RoutedEventArgs e)
        {
            MainWindow.main.mainFrame.Navigate(new CreatePlaylist());
        }

        private async void MoveItemUp(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int itemId)
            {
                if (selectedPlaylist == null) return;

                bool moved = selectedPlaylist.MoveItemUp(itemId);
                if (moved)
                {
                    playlistContentGrid.Items.Refresh();

                    await viewModel.UpdatePlaylistOrderInDatabase(selectedPlaylist);
                }
            }
        }

        private async void MoveItemDown(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int itemId)
            {
                var selectedPlaylist = playlistsListView.SelectedItem as Playlist;
                if (selectedPlaylist == null) return;

                bool moved = selectedPlaylist.MoveItemDown(itemId);

                if (moved)
                {
                    playlistContentGrid.Items.Refresh();

                    await viewModel.UpdatePlaylistOrderInDatabase(selectedPlaylist);
                }
            }
        }

        private void AddMediaFiles(object sender, RoutedEventArgs e)
        {
            if (selectedPlaylist != null)
                MainWindow.main.mainFrame.Navigate(new AddMedia(selectedPlaylist));
        }

        private async void DeleteSelectedPlaylist(object sender, RoutedEventArgs e)
        {
            if (selectedPlaylist == null)
                return;

            var result = MessageBox.Show(
                $"Вы уверены, что хотите удалить плейлист {selectedPlaylist.Name}?\n\n" +
                "Это действие нельзя отменить.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            playlistsListView.SelectionChanged -= Playlist_Selected;

            try
            {
                var success = await viewModel.DeletePlaylistAsync(selectedPlaylist);

                if (success)
                {
                    MessageBox.Show($"Плейлисты успешно удалены", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    playlistsListView.SelectedItem = null;
                    selectedPlaylist = null;

                    await viewModel.LoadPlaylistsAsync();

                    playlistContentGrid.ItemsSource = null;
                    currentPlaylistName.Text = "Выберите плейлист";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                playlistsListView.SelectionChanged += Playlist_Selected;
            }
        }

        private void ConfigurePlaylist(object sender, RoutedEventArgs e)
        {
            MainWindow.main.mainFrame.Navigate(new PlaylistSettings());
        }


        private async void RemoveItem(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int itemId)
            {
                try
                {
                    int playlistId = selectedPlaylist?.Id ?? 0;
                    if (playlistId == 0)
                    {
                        MessageBox.Show("Не выбран плейлист");
                        return;
                    }

                    var result = MessageBox.Show(
                        "Вы уверены, что хотите удалить этот элемент из плейлиста?",
                        "Подтверждение удаления",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result != MessageBoxResult.Yes)
                        return;

                    var connection = new Classes.Common.Connection();
                    await connection.DeletePlaylistItemAsync(itemId);

                    await RefreshPlaylistData(playlistId);
                }
                catch  { }
            }
        }

        private async Task RefreshPlaylistData(int playlistId)
        {
            try
            {
                await viewModel.LoadPlaylistsAsync();

                var updatedPlaylist = viewModel.Playlists.FirstOrDefault(p => p.Id == playlistId);
                if (updatedPlaylist != null)
                {
                    selectedPlaylist = updatedPlaylist;

                    playlistsListView.SelectedItem = selectedPlaylist;

                    playlistContentGrid.ItemsSource = selectedPlaylist.Items;
                    playlistContentGrid.Items.Refresh();

                    currentPlaylistInfo.Text = $"{selectedPlaylist.ItemCount} элементов • {selectedPlaylist.Duration}";
                }
            }
            catch {}
        }

        private void AddWebContent(object sender, RoutedEventArgs e)
        {

        }

        private void AddPlaylist(object sender, RoutedEventArgs e)
        {

        }

        private void DeletePlaylist(object sender, RoutedEventArgs e)
        {

        }

        private void AddFile(object sender, RoutedEventArgs e)
        {

        }

        private void AddUrl(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveFromPlaylist(object sender, RoutedEventArgs e)
        {

        }

        private void StartPlayback(object sender, RoutedEventArgs e)
        {

        }

        private void StopPlayback(object sender, RoutedEventArgs e)
        {

        }

        private void Settings(object sender, RoutedEventArgs e)
        {

        }

        private void Close(object sender, RoutedEventArgs e)
        {

        }
    }
}
