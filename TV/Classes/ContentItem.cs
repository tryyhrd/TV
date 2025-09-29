namespace TV.Classes
{
    public class ContentItem
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Duration { get; set; }
        public long Size { get; set; }
        public string FilePath { get; set; }
    }
}
