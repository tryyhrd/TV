using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using TV.Pages;

namespace TV.Classes.Display
{
    public class DisplayContent
    {
        public int Id { get; set; }
        public int DisplayId { get; set; }
        public string ContentMode { get; set; } 
        public string ContentType { get; set; } 
        public string ContentValue { get; set; } 
        public int? PlaylistId { get; set; }
        public int? ScheduleId { get; set; }
        public string Name { get; set; }
        public int? DisplayDuration { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public bool IsLoop { get; set; }
        public int Priority { get; set; }
        public bool IsActive { get; set; }

        public Playlist Playlist { get; set; }
        public Schedule.Schedule Schedule { get; set; }

        public async Task SaveToDatabaseAsync()
        {
            using (var connection = new MySqlConnection(Common.Connection.connectionString))
            {
                await connection.OpenAsync();

                var deleteCommand = new MySqlCommand(
                    "DELETE FROM DisplayContent WHERE DisplayId = @DisplayId",
                    connection);

                deleteCommand.Parameters.AddWithValue("@DisplayId", DisplayId);
                await deleteCommand.ExecuteNonQueryAsync();

                var insertCommand = new MySqlCommand(@"INSERT INTO DisplayContent 
            (DisplayId, ContentMode, ContentType, ContentValue, PlaylistId, ScheduleId, 
             Name, DisplayDuration, StartDateTime, EndDateTime, IsLoop, Priority, IsActive)
            VALUES 
            (@DisplayId, @ContentMode, @ContentType, @ContentValue, @PlaylistId, @ScheduleId,
             @Name, @DisplayDuration, @StartDateTime, @EndDateTime, @IsLoop, @Priority, @IsActive)",
                    connection);

                insertCommand.Parameters.AddWithValue("@DisplayId", DisplayId);
                insertCommand.Parameters.AddWithValue("@ContentMode", ContentMode);
                insertCommand.Parameters.AddWithValue("@Name", Name);
                insertCommand.Parameters.AddWithValue("@IsLoop", IsLoop);
                insertCommand.Parameters.AddWithValue("@Priority", Priority);
                insertCommand.Parameters.AddWithValue("@IsActive", IsActive);

                if (!string.IsNullOrEmpty(ContentType))
                    insertCommand.Parameters.AddWithValue("@ContentType", ContentType);
                else
                    insertCommand.Parameters.AddWithValue("@ContentType", DBNull.Value);

                if (!string.IsNullOrEmpty(ContentValue))
                    insertCommand.Parameters.AddWithValue("@ContentValue", ContentValue);
                else
                    insertCommand.Parameters.AddWithValue("@ContentValue", DBNull.Value);

                if (PlaylistId.HasValue)
                    insertCommand.Parameters.AddWithValue("@PlaylistId", PlaylistId.Value);
                else
                    insertCommand.Parameters.AddWithValue("@PlaylistId", DBNull.Value);

                if (ScheduleId.HasValue)
                    insertCommand.Parameters.AddWithValue("@ScheduleId", ScheduleId.Value);
                else
                    insertCommand.Parameters.AddWithValue("@ScheduleId", DBNull.Value);

                if (DisplayDuration.HasValue)
                    insertCommand.Parameters.AddWithValue("@DisplayDuration", DisplayDuration.Value);
                else
                    insertCommand.Parameters.AddWithValue("@DisplayDuration", DBNull.Value);

                if (StartDateTime.HasValue)
                    insertCommand.Parameters.AddWithValue("@StartDateTime", StartDateTime.Value);
                else
                    insertCommand.Parameters.AddWithValue("@StartDateTime", DBNull.Value);

                if (EndDateTime.HasValue)
                    insertCommand.Parameters.AddWithValue("@EndDateTime", EndDateTime.Value);
                else
                    insertCommand.Parameters.AddWithValue("@EndDateTime", DBNull.Value);

                await insertCommand.ExecuteNonQueryAsync();
            }
        }

        public static async Task<DisplayContent> LoadByDisplayIdAsync(int displayId)
        {
            using (var connection = new MySqlConnection(Common.Connection.connectionString))
            {
                await connection.OpenAsync();

                var query = @"SELECT 
                            dc.Id,
                            dc.DisplayId,
                            dc.ContentMode,
                            dc.ContentType,
                            dc.ContentValue,
                            dc.PlaylistId,
                            dc.ScheduleId,
                            dc.Name,
                            dc.DisplayDuration,
                            dc.StartDateTime,
                            dc.EndDateTime,
                            dc.IsLoop,
                            dc.Priority,
                            dc.IsActive,
                            p.Id as PlaylistTableId,
                            p.Name as PlaylistName,
                            p.Description as PlaylistDescription,
                            p.IsActive as PlaylistIsActive,
                            p.TotalDuration as PlaylistTotalDuration,
                            s.Id as ScheduleTableId,
                            s.Name as ScheduleName,
                            s.ScheduleType,
                            s.StartTime,
                            s.EndTime,
                            s.DaysOfWeek,
                            s.IsActive as ScheduleIsActive
                            FROM DisplayContent dc
                            LEFT JOIN Playlists p ON dc.PlaylistId = p.Id
                            LEFT JOIN Schedules s ON dc.ScheduleId = s.Id
                            WHERE dc.DisplayId = @DisplayId 
                            AND dc.IsActive = 1
                            ORDER BY dc.Priority DESC 
                            LIMIT 1";

                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@DisplayId", displayId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        var ordinals = new
                        {
                            Id = reader.GetOrdinal("Id"),
                            DisplayId = reader.GetOrdinal("DisplayId"),
                            ContentMode = reader.GetOrdinal("ContentMode"),
                            ContentType = reader.GetOrdinal("ContentType"),
                            ContentValue = reader.GetOrdinal("ContentValue"),
                            PlaylistId = reader.GetOrdinal("PlaylistId"),
                            ScheduleId = reader.GetOrdinal("ScheduleId"),
                            Name = reader.GetOrdinal("Name"),
                            DisplayDuration = reader.GetOrdinal("DisplayDuration"),
                            StartDateTime = reader.GetOrdinal("StartDateTime"),
                            EndDateTime = reader.GetOrdinal("EndDateTime"),
                            IsLoop = reader.GetOrdinal("IsLoop"),
                            Priority = reader.GetOrdinal("Priority"),
                            IsActive = reader.GetOrdinal("IsActive"),
                            PlaylistTableId = reader.GetOrdinal("PlaylistTableId"),
                            PlaylistName = reader.GetOrdinal("PlaylistName"),
                            PlaylistDescription = reader.GetOrdinal("PlaylistDescription"),
                            PlaylistIsActive = reader.GetOrdinal("PlaylistIsActive"),
                            PlaylistTotalDuration = reader.GetOrdinal("PlaylistTotalDuration"),
                            ScheduleTableId = reader.GetOrdinal("ScheduleTableId"),
                            ScheduleName = reader.GetOrdinal("ScheduleName"),
                            ScheduleType = reader.GetOrdinal("ScheduleType"),
                            StartTime = reader.GetOrdinal("StartTime"),
                            EndTime = reader.GetOrdinal("EndTime"),
                            DaysOfWeek = reader.GetOrdinal("DaysOfWeek"),
                            ScheduleIsActive = reader.GetOrdinal("ScheduleIsActive")
                        };

                        var displayContent = new DisplayContent
                        {
                            Id = reader.GetInt32(ordinals.Id),
                            DisplayId = reader.GetInt32(ordinals.DisplayId),
                            ContentMode = reader.GetString(ordinals.ContentMode),
                            Name = reader.GetString(ordinals.Name),
                            IsLoop = reader.GetBoolean(ordinals.IsLoop),
                            Priority = reader.GetInt32(ordinals.Priority),
                            IsActive = reader.GetBoolean(ordinals.IsActive)
                        };

                        if (reader.IsDBNull(ordinals.DisplayDuration))
                        {
                            displayContent.DisplayDuration = null;
                        }
                        else
                        {
                            displayContent.DisplayDuration = reader.GetInt32(ordinals.DisplayDuration);
                        }

                        if (reader.IsDBNull(ordinals.StartDateTime))
                        {
                            displayContent.StartDateTime = null;
                        }
                        else
                        {
                            displayContent.StartDateTime = reader.GetDateTime(ordinals.StartDateTime);
                        }

                        if (reader.IsDBNull(ordinals.EndDateTime))
                        {
                            displayContent.EndDateTime = null;
                        }
                        else
                        {
                            displayContent.EndDateTime = reader.GetDateTime(ordinals.EndDateTime);
                        }

                        if (!reader.IsDBNull(ordinals.ContentType))
                        {
                            displayContent.ContentType = reader.GetString(ordinals.ContentType);

                            if (!reader.IsDBNull(ordinals.ContentValue))
                            {
                                displayContent.ContentValue = reader.GetString(ordinals.ContentValue);
                            }
                            else
                            {
                                displayContent.ContentValue = null;
                            }
                        }

                        if (!reader.IsDBNull(ordinals.PlaylistId))
                        {
                            displayContent.PlaylistId = reader.GetInt32(ordinals.PlaylistId);
                            displayContent.Playlist = new Playlist
                            {
                                Id = reader.GetInt32(ordinals.PlaylistTableId),
                                Name = reader.GetString(ordinals.PlaylistName),
                                IsActive = reader.GetBoolean(ordinals.PlaylistIsActive)
                            };

                            if (!reader.IsDBNull(ordinals.PlaylistDescription))
                            {
                                displayContent.Playlist.Description = reader.GetString(ordinals.PlaylistDescription);
                            }
                        }

                        if (!reader.IsDBNull(ordinals.ScheduleId))
                        {
                            displayContent.ScheduleId = reader.GetInt32(ordinals.ScheduleId);

                            var schedule = new Schedule.Schedule
                            {
                                Id = reader.GetInt32(ordinals.ScheduleTableId),
                                Name = reader.GetString(ordinals.ScheduleName),
                                ScheduleType = reader.GetString(ordinals.ScheduleType),
                                IsActive = reader.GetBoolean(ordinals.ScheduleIsActive)
                            };

                            if (!reader.IsDBNull(ordinals.DaysOfWeek))
                            {
                                schedule.DaysOfWeek = reader.GetString(ordinals.DaysOfWeek);
                            }

                            if (!reader.IsDBNull(ordinals.StartTime))
                            {
                                schedule.StartTime = reader.GetTimeSpan(ordinals.StartTime);
                            }

                            if (!reader.IsDBNull(ordinals.EndTime))
                            {
                                schedule.EndTime = reader.GetTimeSpan(ordinals.EndTime);
                            }

                            displayContent.Schedule = schedule;
                        }

                        if (!reader.IsDBNull(ordinals.PlaylistId))
                        {
                            var connectionClass = new Common.Connection();
                            displayContent.PlaylistId = reader.GetInt32(ordinals.PlaylistId);
                            displayContent.Playlist = new Playlist
                            {
                                Id = reader.GetInt32(ordinals.PlaylistTableId),
                                Name = reader.GetString(ordinals.PlaylistName),
                                Description = reader.IsDBNull(ordinals.PlaylistDescription) ? null : reader.GetString(ordinals.PlaylistDescription),
                                IsActive = reader.GetBoolean(ordinals.PlaylistIsActive),
                                Items = await connectionClass.GetPlaylistItemsAsync(displayContent.Playlist.Id)
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
