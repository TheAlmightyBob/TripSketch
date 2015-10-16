using SQLite;
using System;

namespace TripSketch.Core.Models
{
    // TODO: Rename to something less-likely to conflict with programming terminology...
    //       - CalendarEvent? (longish...)
    //       - Appointment? (used by MS, but not really the type of event that TripSketch is about...)
    //       - Activity? (possibly confusing in an Android context?)
    //
    public class Event : TimeRange
    {
        #region Fields

        private string _name;
        private bool _allDay = true;
        private string _externalID;

        #endregion

        #region DB Properties

        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }

        public int ItineraryID { get; set; }

        #endregion

        #region Properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    HasChanged();
                }
            }
        }

        public bool AllDay
        {
            get { return _allDay; }
            set
            {
                if (_allDay != value)
                {
                    _allDay = value;
                    HasChanged();
                    HasChanged(nameof(Start), nameof(End), nameof(StartDate), nameof(EndDate));
                }
            }
        }

        /// <summary>
        /// ID used by external calendar service, if this event has been imported/exported.
        /// Note that this will be the same for recurring events.
        /// </summary>
        public string ExternalID
        {
            get { return _externalID; }
            set
            {
                if (_externalID != value)
                {
                    _externalID = value;
                    HasChanged();
                }
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return "Name=" + Name + ", AllDay=" + AllDay + ", Start=" + Start + ", End=" + End;
        }

        #endregion

        #region Constructor

        // Default initializes to one day
        //
        public Event() : base(DateTime.Today, DateTime.Today.AddDays(1))
        {
        }

        #endregion
    }
}
