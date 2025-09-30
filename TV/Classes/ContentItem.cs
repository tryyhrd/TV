namespace TV.Classes
{
    public class ContentItem
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int Order {  get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Duration { get; set; }
        public long Size { get; set; }
        public string FilePath { get; set; }
    }
}
