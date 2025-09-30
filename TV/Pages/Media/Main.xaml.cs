using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
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

        private async void Playlist_Selected(object sender, SelectionChangedEventArgs e)
        {
            selectedPlaylist = playlistsListView.SelectedItem as Playlist;

            var connection = new Classes.Common.Connection();
            var items = await connection.GetPlaylistItemsAsync(selectedPlaylist.Id);

            if (items != null)
                playlistContentGrid.ItemsSource = items;
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

        private void MoveUp(object sender, RoutedEventArgs e)
        {

        }

        private void MoveDown(object sender, RoutedEventArgs e)
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

        private void CreatePlaylist(object sender, RoutedEventArgs e)
        {
            MainWindow.main.mainFrame.Navigate(new CreatePlaylist());
        }

        private void DeleteSelectedPlaylists(object sender, RoutedEventArgs e)
        {

        }

        private void ImportPlaylist(object sender, RoutedEventArgs e)
        {

        }

        private void ExportPlaylist(object sender, RoutedEventArgs e)
        {

        }

        private void RenamePlaylist(object sender, RoutedEventArgs e)
        {

        }

        private void TogglePreview(object sender, RoutedEventArgs e)
        {

        }

        private void MoveItemUp(object sender, RoutedEventArgs e)
        {

        }

        private void MoveItemDown(object sender, RoutedEventArgs e)
        {

        }

        private void RemoveItem(object sender, RoutedEventArgs e)
        {

        }

        private void AddMediaFiles(object sender, RoutedEventArgs e)
        {
            if (selectedPlaylist != null)
                MainWindow.main.mainFrame.Navigate(new AddMedia(selectedPlaylist));
        }

        private void AddWebContent(object sender, RoutedEventArgs e)
        {

        }

        private void ConfigureTiming(object sender, RoutedEventArgs e)
        {

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
    }
}
