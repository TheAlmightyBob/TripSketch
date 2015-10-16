using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TripSketch.Core.Models;

namespace TripSketch.Core.Services
{
    public interface ICalendarService
    {
        Task<ObservableCollection<Calendar>> GetCalendarsAsync(bool writeable = false);
        Task<ObservableCollection<Event>> GetEventsAsync(Calendar calendar, DateTime start, DateTime end);

        /// <summary>
        /// Exports events to the specified Calendar, creating it if necessary (i.e., if DeviceID is missing).
        /// If using an existing Calendar, note that it must be writeable (else this will throw).
        /// </summary>
        /// <param name="calendar"></param>
        /// <param name="events"></param>
        /// <returns></returns>
        Task ExportEventsAsync(Calendar calendar, IList<Event> events);
    }
}
