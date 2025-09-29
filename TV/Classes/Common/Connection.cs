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
                    playlists.Add(new Playlist
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1),
                        IsActive = reader.GetBoolean(2)
                    });
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
                    Name = reader.GetString(3),
                    Size = reader.IsDBNull(4) ? 0 : reader.GetInt64(4),
                    FilePath = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }

            return items;
        }
    }
}
