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
        public string Description { get; set; }
        public List<PlaylistItem> Items { get; set; } = new List<PlaylistItem>();

        public int ItemCount => Items?.Count ?? 0;

        public string Duration => Items?.Any() == true ?
        TimeSpan.FromSeconds(Items.Sum(item => item.Duration)).ToString(@"hh\:mm\:ss") : "00:00:00";

        public bool MoveItemUp(int itemId)
        {
            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;

            int currentIndex = Items.IndexOf(item);
            if (currentIndex <= 0) return false;

            var previousItem = Items[currentIndex - 1];
            Items[currentIndex - 1] = item;
            Items[currentIndex] = previousItem;

            UpdateItemOrders();
            return true;
        }

        public bool MoveItemDown(int itemId)
        {
            var item = Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null) return false;

            int currentIndex = Items.IndexOf(item);
            if (currentIndex >= Items.Count - 1) return false;

            var nextItem = Items[currentIndex + 1];
            Items[currentIndex + 1] = item;
            Items[currentIndex] = nextItem;

            UpdateItemOrders();
            return true;
        }

        private void UpdateItemOrders()
        {
            for (int i = 0; i < Items.Count; i++)
            {
                Items[i].Order = i + 1;
            }
        }
    }
}
