using System;
using System.Collections.ObjectModel;

namespace TripSketch.Core.Models
{
    public class EventsForDay : ObservableCollection<Event>
    {
        public DateTime Date { get; set; }
    }
}
