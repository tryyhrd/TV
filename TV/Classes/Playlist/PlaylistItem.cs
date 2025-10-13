using System;

namespace TV.Classes
{
    public class PlaylistItem
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int Order {  get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Duration { get; set; }
        public long Size { get; set; }
        public string FilePath { get; set; }

        public string FormattedSize
        {
            get
            {
                if (Size == 0) return "0 B";

                string[] sizes = { "B", "KB", "MB", "GB" };
                int order = 0;
                double len = Size;
                while (len >= 1024 && order < sizes.Length - 1)
                {
                    order++;
                    len /= 1024;
                }
                return $"{len:0.##} {sizes[order]}";
            }
        }

        public string FormattedDuration
        {
            get
            {
               return TimeSpan.FromSeconds(Duration).ToString(@"hh\:mm\:ss");
            }  
        }   
    }
}
