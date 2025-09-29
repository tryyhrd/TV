using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
    }
}
