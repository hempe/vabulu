using System;

namespace Vabulu.Models {
    public class CalendarEvent {
        public string PropertyId { get; set; }
        public string Id { get; set; }
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public string Title { get; set; }
        public EventColor Color { get; set; }
        public CalendarEventData Meta { get; set; }
    }

    public class EventColor {
        public string Primary { get; set; }
        public string Secondary { get; set; }
    }

    public class CalendarEventData {
        public string Comment { get; set; }
    }
}