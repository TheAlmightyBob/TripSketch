using System.Collections.ObjectModel;
using TripSketch.Core.Helpers;

namespace TripSketch.Core.Models
{
    public class Itinerary : DBModel //: PropertyChangeNotifier
    {
        public string Name { get; set; }

        public int TripID { get; set; }


        //private ObservableCollection<Event> _events;

        //public ObservableCollection<Event> Events
        //{
        //    get { return _events; }
        //    set
        //    {
        //        if (_events != value)
        //        {
        //            _events = value;
        //            HasChanged();
        //        }
        //    }
        //}
    }
}
