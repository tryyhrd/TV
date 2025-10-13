namespace TV.Classes.Schedule
{
    public class ScheduleItem
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public string ContentType { get; set; }
        public int? PlaylistId { get; set; }
        public string MediaPath { get; set; }
        public string WebUrl { get; set; }
        public int Duration { get; set; }
        public int Order { get; set; }

        public string PlaylistName { get; set; }
    }
}
