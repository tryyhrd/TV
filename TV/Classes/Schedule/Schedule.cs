using System;

namespace TV.Classes.Schedule
{
    public class Schedule
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ScheduleType { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string DaysOfWeek { get; set; }
        public bool IsActive { get; set; }
    }
}
