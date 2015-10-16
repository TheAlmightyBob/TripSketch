using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TripSketch.Core.Helpers;

namespace TripSketch.Core.Models
{
    public class Trip : DBModel //: PropertyChangeNotifier
    {
        public string Name { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public string DateRangeDisplay { get { return $"{Start.ToString("d")} - {End.ToString("d")}"; } }

        //public List<Itinerary> Itineraries { get; set; }

        //#region Fields

        //private DateTime _start;
        //private DateTime _end;
        //private ObservableCollection<Itinerary> _itineraries;

        //#endregion

        //#region Properties

        //public DateTime Start
        //{
        //    get { return _start; }
        //    set
        //    {
        //        if (_start != value)
        //        {
        //            _start = value;
        //            HasChanged();
        //        }
        //    }
        //}

        //public DateTime End
        //{
        //    get { return _end; }
        //    set
        //    {
        //        if (_end != value)
        //        {
        //            _end = value;
        //            HasChanged();
        //        }
        //    }
        //}

        //public ObservableCollection<Itinerary> Itineraries
        //{
        //    get { return _itineraries; }
        //    set
        //    {
        //        if (_itineraries != value)
        //        {
        //            _itineraries = value;
        //            HasChanged();
        //        }
        //    }
        //}

        //#endregion
    }
}
