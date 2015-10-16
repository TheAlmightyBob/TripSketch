using Calendars.Plugin;
using Calendars.Plugin.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using TripSketch.Core.Models;

namespace TripSketch.Core.Services
{
    /// <summary>
    /// Wraps Calendars API instead of using TripSketch's own platform-specific implementations
    /// </summary>
    public class CalendarService : ICalendarService
    {
        public async Task<ObservableCollection<TripSketch.Core.Models.Calendar>> GetCalendarsAsync(bool writeable = false)
        {
            var calendars = await CrossCalendars.Current.GetCalendarsAsync().ConfigureAwait(false);

            return new ObservableCollection<TripSketch.Core.Models.Calendar>(calendars
                .Where(c => !writeable || (c.CanEditCalendar && c.CanEditEvents))
                .Select(c => new TripSketch.Core.Models.Calendar
                {
                    Name = c.Name,
                    ExternalID = c.ExternalID
                }));
        }

        public async Task<ObservableCollection<Event>> GetEventsAsync(TripSketch.Core.Models.Calendar calendar, DateTime start, DateTime end)
        {
            var apiCalendar = new Calendars.Plugin.Abstractions.Calendar { Name = calendar.Name, ExternalID = calendar.ExternalID };
            var events = await CrossCalendars.Current.GetEventsAsync(apiCalendar, start, end).ConfigureAwait(false);

            return new ObservableCollection<Event>(events.Select(e => new Event
                {
                    Name = e.Name,
                    AllDay = e.AllDay,
                    ExternalID = e.ExternalID,
                    Start = e.Start,
                    End = e.End
                }));
        }

        public async Task ExportEventsAsync(TripSketch.Core.Models.Calendar calendar, IList<Event> events)
        {
            var apiCalendar = new Calendars.Plugin.Abstractions.Calendar { Name = calendar.Name, ExternalID = calendar.ExternalID };
            var calendarEvents = events.Select(e => new CalendarEvent
                {
                    Name = e.Name,
                    AllDay = e.AllDay,
                    ExternalID = e.ExternalID,
                    Start = e.Start,
                    End = e.End
                });

            if (string.IsNullOrEmpty(apiCalendar.ExternalID))
            {
                await CrossCalendars.Current.AddOrUpdateCalendarAsync(apiCalendar).ConfigureAwait(false);
                calendar.ExternalID = apiCalendar.ExternalID;
            }

            foreach (var ev in events)
            {
                var calendarEvent = new CalendarEvent
                {
                    Name = ev.Name,
                    AllDay = ev.AllDay,
                    ExternalID = ev.ExternalID,
                    Start = ev.Start,
                    End = ev.End
                };

                await CrossCalendars.Current.AddOrUpdateEventAsync(apiCalendar, calendarEvent).ConfigureAwait(false);

                ev.ExternalID = calendarEvent.ExternalID;
            }
        }
    }
}
