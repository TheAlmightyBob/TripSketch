using System;
using TripSketch.Core.Helpers;

namespace TripSketch.Core.Models
{
    public class TimeRange : PropertyChangeNotifier
    {
        // TODO:
        // - Validation in Start/End setters?
        // - "Move" function?

        #region Fields

        private DateTime _start;
        private DateTime _end;

        #endregion

        #region Properties

        public DateTime Start
        {
            get { return _start; }
            set
            {
                if (_start != value)
                {
                    _start = value;
                    HasChanged();
                    HasChanged(nameof(StartDate));
                }
            }
        }

        public DateTime End
        {
            get { return _end; }
            set
            {
                if (_end != value)
                {
                    _end = value;
                    HasChanged();
                    HasChanged(nameof(EndDate));
                }
            }
        }

        /// <summary>
        /// Setting StartDate will automatically adjust the specified date to ensure
        /// that applying it preserves the Start-before-End relationship.
        /// </summary>
        public DateTime StartDate
        {
            get { return _start.Date; }
            set
            {
                var newStart = value.Add(_start.TimeOfDay);

                if (_end <= newStart)
                {
                    // Adjust date to ensure that end does not precede start.
                    // The +1 is rounding to account for differences of less than a day.
                    // (or even exactly one day, in which case only subtracting one day would make them equal,
                    //  which is still invalid)
                    //
                    newStart = newStart.AddDays(-((newStart - _end).Days + 1));
                }

                Start = newStart;
            }
        }

        /// <summary>
        /// This is different from End.Date because this handles the fact that an end time
        /// of midnight should really be considered the end of the previous day.
        /// Setting EndDate will automatically adjust the specified date to ensure that
        /// applying it preserves the Start-before-End relationship.
        /// </summary>
        public DateTime EndDate
        {
            // An End time of midnight is really the end of the previous day
            //
            get { return End.Date.AddDays(End.TimeOfDay == TimeSpan.Zero ? -1 : 0); }
            set
            {
                var newEnd = value.Add(_end.TimeOfDay == TimeSpan.Zero ? TimeSpan.FromDays(1) : _end.TimeOfDay);

                if (newEnd <= _start)
                {
                    // Adjust date to ensure that end does not precede start.
                    // The +1 is rounding up to account for differences of less than a day.
                    // (or even exactly one day, in which case only adding one day would make them equal,
                    //  which is still invalid)
                    //
                    newEnd = newEnd.AddDays((_start - newEnd).Days + 1);
                }

                End = newEnd;
            }
        }

        public TimeSpan Duration
        {
            get { return _end - _start; }
        }

        /// <summary>
        /// This is not how many 24-hour periods, but rather how many
        /// days this range intersects (e.g., must be at least one)
        /// </summary>
        public int DaySpan
        {
            get { return (EndDate - StartDate).Days + 1; }
        }

        #endregion

        #region Constructor

        public TimeRange(DateTime start, DateTime end)
        {
            _start = start;
            _end = end;
        }

        #endregion

        #region Public Methods

        public void MoveToDate(DateTime newStartDate)
        {
            var duration = Duration;
            Start = newStartDate.Add(Start.TimeOfDay);
            End = Start.Add(duration);
        }

        #endregion
    }
}
