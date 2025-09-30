using System;
using System.Collections.Generic;
using System.Linq;

namespace TV.Classes
{
    public class Playlist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; } = false;
        public List<ContentItem> Items { get; set; } = new List<ContentItem>();

        public int ItemCount => Items?.Count ?? 0;

        public string Duration => Items?.Any() == true ?
        TimeSpan.FromSeconds(Items.Sum(item => item.Duration)).ToString(@"hh\:mm\:ss") : "00:00:00";
    }
}
