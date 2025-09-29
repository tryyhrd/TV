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

        public async Task LoadPlaylistsAsync()
        {
            var connection = new Common.Connection();
            Playlists = await connection.GetPlaylistsAsync();
        }
    }
}
