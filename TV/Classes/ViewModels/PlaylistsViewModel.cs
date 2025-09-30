﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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
    }
}
