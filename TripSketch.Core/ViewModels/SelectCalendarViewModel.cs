using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TripSketch.Core.Models;
using TripSketch.Core.Enums;

namespace TripSketch.Core.ViewModels
{
    public class SelectCalendarViewModel : ModalViewModelBase
    {
        #region Fields

        private ObservableCollection<Calendar> _calendars;
        private Calendar _selectedCalendar;

        #endregion

        #region Properties

        public string Title { get { return "Select Calendar"; } }

        public ObservableCollection<Calendar> Calendars
        {
            get { return _calendars; }
            set
            {
                if (_calendars != value)
                {
                    _calendars = value;
                    HasChanged();
                }
            }
        }

        public Calendar SelectedCalendar
        {
            get { return _selectedCalendar; }
            set
            {
                if (_selectedCalendar != value)
                {
                    _selectedCalendar = value;
                    HasChanged();

                    Result = ModalResult.Done;
                    Navigator.PopModalAsync();
                }
            }
        }

        #endregion

        public override void Initialize()
        {
            FetchCalendars();
        }

        private async void FetchCalendars()
        {
            Debug.Assert(CalendarService != null);

            if (CalendarService != null)
            {
                try
                {
                    Calendars = await CalendarService.GetCalendarsAsync();
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            }
        }
    }
}
