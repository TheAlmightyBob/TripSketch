using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TripSketch.Core.Models;
using TripSketch.Core.Services;

namespace TripSketch.Mocks
{
    public class MockCalendarService : ICalendarService
    {
        public Func<ObservableCollection<Calendar>> GetCalendarsImpl { get; set; }

        public Task<ObservableCollection<Calendar>> GetCalendarsAsync(bool writeable = false)
        {
            if (GetCalendarsImpl != null)
            {
                return Task.FromResult(GetCalendarsImpl());
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Func<Calendar, DateTime, DateTime, ObservableCollection<Event>> GetEventsImpl { get; set; }

        public Task<ObservableCollection<Event>> GetEventsAsync(Calendar calendar, DateTime start, DateTime end)
        {
            if (GetEventsImpl != null)
            {
                return Task.FromResult(GetEventsImpl(calendar, start, end));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public Action<Calendar, IList<Event>> ExportEventsImpl { get; set; }

        public Task ExportEventsAsync(Calendar calendar, IList<Event> events)
        {
            if (ExportEventsImpl != null)
            {
                ExportEventsImpl(calendar, events);
                return Task.FromResult(0);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
