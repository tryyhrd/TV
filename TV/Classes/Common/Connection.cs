using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TV.Classes.Common
{
    public class Connection
    {
        public static string connectionString = "server=localhost;port=3306;database=TV;uid=root;pwd=;charset=utf8;";

        public async Task<List<Playlist>> GetPlaylistsAsync()
        {
            var playlists = new List<Playlist>();

            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT * FROM playlists";

                var command = new MySqlCommand(query, connection);

                var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var playlist = new Playlist
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        IsActive = reader.GetBoolean(2)
                    };

                    playlist.Items = await GetPlaylistItemsAsync(playlist.Id);

                    playlists.Add(playlist);
                }
            }

            return playlists;
        }
        public async Task<List<ContentItem>> GetPlaylistItemsAsync(int playlistId)
        {
            var items = new List<ContentItem>();

            var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"SELECT * FROM PlaylistItems 
                     WHERE PlaylistId = @playlistId 
                     ORDER BY OrderIndex ASC"
            ;

            var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@playlistId", playlistId);

            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                items.Add(new ContentItem
                {
                    Id = reader.GetInt32(0),
                    PlaylistId = reader.GetInt32(1),
                    Order = reader.GetInt32(2),
                    Name = reader.GetString(3),
                    Type = reader.GetString(4),
                    Duration = reader.GetInt32(5),
                    Size = reader.IsDBNull(6) ? 0 : reader.GetInt64(6),
                    FilePath = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
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
    }
}
