using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using TripSketch.Core.Enums;
using TripSketch.Core.Models;
using Xamarin.Forms;

namespace TripSketch.Core.ViewModels
{
    public class WriteableCalendarsViewModel : ModalViewModelBase
    {
        #region Fields

        private ObservableCollection<Calendar> _calendars;
        private Calendar _selectedCalendar;

        #endregion

        #region Properties

        public string Title { get { return "Select Destination"; } }

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

        public ICommand AddCommand { get { return new Command(AddCalendar); } }

        #endregion

        public override void Initialize()
        {
            // If not null, we have been pre-initialized
            //
            if (_calendars == null)
            {
                FetchCalendars();
            }
        }

        private async void FetchCalendars()
        {
            Debug.Assert(CalendarService != null);

            if (CalendarService != null)
            {
                try
                {
                    Calendars = await CalendarService.GetCalendarsAsync(true);
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            }
        }

        private async void AddCalendar()
        {
            Debug.Assert(CalendarService != null);

            if (CalendarService != null)
            {
                try
                {
                    var newCalVM = await Navigator.PushModalAndWaitAsync<NewCalendarViewModel>();

                    if (newCalVM.Result != ModalResult.Canceled)
                    {
                        Calendars.Add(new Calendar { Name = newCalVM.CalendarName });
                    }
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            }
        }
    }
}
