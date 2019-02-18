using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.WindowsAzure.Storage.Table;
using Vabulu.Attributes;

namespace Vabulu.Tables
{
    [Table("CalendarEvents")]
    public class CalendarEvent : TableEntity
    {

        [IgnoreProperty]
        [RowKey]
        public string Id { get; set; }

        [IgnoreProperty]
        [PartitionKey]
        public string PropertyId { get; set; }

        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }

        public string Title { get; set; }

        [IgnoreProperty]
        [JsonData(nameof(ColorString))]
        public Models.EventColor Color { get; set; }
        public string ColorString { get; set; }

        [IgnoreProperty]
        [JsonData(nameof(MetaString))]
        public Models.CalendarEventData Meta { get; set; }
        public string MetaString { get; set; }

        public static implicit operator CalendarEvent(Models.CalendarEvent x) => x == null ? null : new CalendarEvent
        {
            PropertyId = x.PropertyId,
            Id = x.Id,
            Start = x.Start,
            End = x.End,
            Title = x.Title,
            Color = x.Color,
            Meta = x.Meta
        };

        public static implicit operator Models.CalendarEvent(CalendarEvent x) => x == null ? null : new Models.CalendarEvent
        {
            PropertyId = x.PropertyId,
            Id = x.Id,
            Start = x.Start,
            End = x.End,
            Title = x.Title,
            Color = x.Color,
            Meta = x.Meta
        };
    }
}