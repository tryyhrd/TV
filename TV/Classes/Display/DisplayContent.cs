using MySql.Data.MySqlClient;
using System;
using System.Threading.Tasks;

namespace TV.Classes.Display
{
    public class DisplayContent
    {
        public int Id { get; set; }
        public int DisplayId { get; set; }
        public string ContentType { get; set; } 
        public string ContentValue { get; set; }
        public string DisplayName { get; set; }

        public Playlist Playlist { get; set; }

        public int? PlaylistId { get; set; }

        public async Task SaveToDatabaseAsync()
        {
            using (var connection = new MySqlConnection(Common.Connection.connectionString)) 
            {
                await connection.OpenAsync();

                var deleteCommand = new MySqlCommand(
                    "DELETE FROM DisplayContent WHERE DisplayId = @DisplayId",
                    connection);

                deleteCommand.Parameters.AddWithValue("@DisplayId", this.DisplayId);
                await deleteCommand.ExecuteNonQueryAsync();

                var insertCommand = new MySqlCommand(@"INSERT INTO DisplayContent 
                                                    (DisplayId, ContentType, ContentValue, DisplayName, PlaylistId)
                                                    VALUES 
                                                    (@DisplayId, @ContentType, @ContentValue, @DisplayName, @PlaylistId)",
                    connection);

                insertCommand.Parameters.AddWithValue("@DisplayId", this.DisplayId);
                insertCommand.Parameters.AddWithValue("@ContentType", this.ContentType);
                insertCommand.Parameters.AddWithValue("@ContentValue", this.ContentValue);
                insertCommand.Parameters.AddWithValue("@DisplayName", this.DisplayName);

                if (Playlist != null)
                    insertCommand.Parameters.AddWithValue("@PlaylistId", this.Playlist.Id);
                else
                    insertCommand.Parameters.AddWithValue("@PlaylistId", DBNull.Value);
                
                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        public static async Task<DisplayContent> LoadByDisplayIdAsync(int displayId)
        {
            using (var connection = new MySqlConnection(Common.Connection.connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT 
                        dc.Id as DisplayContentId,
                        dc.DisplayId,
                        dc.ContentType,
                        dc.ContentValue,
                        dc.DisplayName,
                        dc.PlaylistId,
                        p.Id as PlaylistId,
                        p.Name as PlaylistName
                      FROM DisplayContent dc
                      LEFT JOIN Playlists p ON dc.PlaylistId = p.Id
                      WHERE dc.DisplayId = @DisplayId";

                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@DisplayId", displayId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var displayContent = new DisplayContent
                        {
                            Id = reader.GetInt32(0),
                            DisplayId = reader.GetInt32(1),
                            ContentType = reader.GetString(2),
                            ContentValue = reader.GetString(3),
                            DisplayName = reader.GetString(4)
                        };

                        if (!reader.IsDBNull(reader.GetOrdinal("PlaylistId")))
                        {
                            var playlistId = reader.GetInt32(5);

                            displayContent.PlaylistId = playlistId;
                            displayContent.Playlist = new Playlist
                            {
                                Id = playlistId,
                                Name = reader.GetString(7)
                            };
                        }
                        return displayContent;
                    }
                } 
                return null;
            }    
        }
    }
}
