using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace TV.Classes.ViewModels
{
    public class PlaylistsViewModel: INotifyPropertyChanged
    {
        private List<Playlist> _playlists;
        public List<Playlist> Playlists
        {
            get => _playlists;
            set
            {
                _playlists = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public async Task<List<Playlist>> LoadPlaylistsAsync()
        {
            var connection = new Common.Connection();
            return Playlists = await connection.GetPlaylistsAsync();
        }

        public async Task<bool> DeletePlaylistAsync(Playlist playlist)
        {
            var connectionClass = new Common.Connection();

            using (var connection = new MySqlConnection(Common.Connection.connectionString))
            {
                await connection.OpenAsync();

                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        await connectionClass.DeletePlaylistItemsAsync(playlist.Id, connection, transaction);
                        await connectionClass.DeletePlaylistAsync(playlist.Id, connection, transaction);
                        await transaction.CommitAsync();

                        return true;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task UpdatePlaylistOrderInDatabase(Playlist playlist)
        {
            try
            {
                using (var connection = new MySqlConnection(Common.Connection.connectionString))
                {
                    await connection.OpenAsync();

                    foreach (var item in playlist.Items)
                    {
                        var query = @"UPDATE PlaylistItems 
                             SET `Order` = @order 
                             WHERE Id = @itemId";

                        var command = new MySqlCommand(query, connection);
                        command.Parameters.AddWithValue("@order", item.Order);
                        command.Parameters.AddWithValue("@itemId", item.Id);

                        await command.ExecuteNonQueryAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении порядка: {ex.Message}", "Ошибка",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }   
}
