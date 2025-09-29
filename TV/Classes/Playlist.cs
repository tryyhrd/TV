using System.Collections.Generic;

namespace TV.Classes
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = false;
        public List<ContentItem> Items { get; set; } = new List<ContentItem>();
    }
}
