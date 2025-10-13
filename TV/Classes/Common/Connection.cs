using MySqlConnector;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TV.Classes.Common
{
    public class Connection
    {
        public static string connectionString = "server=localhost;port=3306;database=TVDisplay;uid=root;pwd=;charset=utf8;";

        public async Task<List<Playlist>> GetPlaylistsAsync()
        {
            var playlists = new List<Playlist>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT * FROM Playlists";

                var command = new MySqlCommand(query, connection);

                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var playlist = new Playlist
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        Description = reader.GetString(2),
                        IsActive = reader.GetBoolean(3)
                    };

                    playlist.Items = await GetPlaylistItemsAsync(playlist.Id);

                    playlists.Add(playlist);
                }
            }

            return playlists;
        }
        public async Task<List<PlaylistItem>> GetPlaylistItemsAsync(int playlistId)
        {
            var items = new List<PlaylistItem>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT * FROM PlaylistItems 
                 WHERE PlaylistId = @playlistId 
                 ORDER BY `Order` ASC";

                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@playlistId", playlistId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // Получаем индексы колонок
                        var ordinals = new
                        {
                            Id = reader.GetOrdinal("Id"),
                            PlaylistId = reader.GetOrdinal("PlaylistId"),
                            Order = reader.GetOrdinal("Order"),
                            Name = reader.GetOrdinal("Name"),
                            Type = reader.GetOrdinal("Type"),
                            Duration = reader.GetOrdinal("Duration"),
                            Size = reader.GetOrdinal("Size"),
                            FilePath = reader.GetOrdinal("FilePath")
                        };

                        items.Add(new PlaylistItem
                        {
                            Id = reader.GetInt32(ordinals.Id),
                            PlaylistId = reader.GetInt32(ordinals.PlaylistId),
                            Order = reader.GetInt32(ordinals.Order),
                            Name = reader.GetString(ordinals.Name),
                            Type = reader.GetString(ordinals.Type),
                            Duration = reader.GetInt32(ordinals.Duration),
                            Size = reader.IsDBNull(ordinals.Size) ? 0 : reader.GetInt64(ordinals.Size),
                            FilePath = reader.IsDBNull(ordinals.FilePath) ? null : reader.GetString(ordinals.FilePath)
                        });
                    }
                }
            }

            return items;
        }

        public async Task DeletePlaylistAsync(int playlistId, MySqlConnection connection, MySqlTransaction transaction)
        {
            var deletePlaylistQuery = "DELETE FROM Playlists WHERE Id = @playlistId";

            using (var command = new MySqlCommand(deletePlaylistQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@playlistId", playlistId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeletePlaylistItemsAsync(int playlistId, MySqlConnection connection, MySqlTransaction transaction)
        {
            var deleteItemsQuery = "DELETE FROM PlaylistItems WHERE PlaylistId = @playlistId";

            using (var command = new MySqlCommand(deleteItemsQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@playlistId", playlistId);
                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task DeletePlaylistItemAsync(int itemId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var deleteItemQuery = "DELETE FROM PlaylistItems WHERE Id = @itemId";
                using (var command = new MySqlCommand(deleteItemQuery, connection))
                {
                    command.Parameters.AddWithValue("@itemId", itemId);
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
