using System.Collections.Generic;
using System.Windows.Input;
using System.Linq;
using TripSketch.Core.Enums;
using TripSketch.Core.Models;
using Xamarin.Forms;
using System;

namespace TripSketch.Core.ViewModels
{
    public class NewItineraryViewModel : ModalViewModelBase
    {
        #region Fields

        private string _name;

        private Itinerary _itinerary;

        private EventImportMode _importMode;

        private Command _doneCommand;

        #endregion

        #region Properties

        public string Title { get { return "New Itinerary"; } }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    HasChanged();
                    _doneCommand.ChangeCanExecute();
                }
            }
        }

        public Trip Trip { get; set; }

        public Itinerary Itinerary
        {
            get { return _itinerary; }
            set
            {
                if (_itinerary != value)
                {
                    _itinerary = value;
                    Name = _itinerary.Name;
                }
            }
        }

        public int ImportMode
        {
            get { return (int)_importMode; }
            set
            {
                if (_importMode != (EventImportMode)value)
                {
                    _importMode = (EventImportMode)value;
                    HasChanged();
                }
            }
        }

        // Picker doesn't support binding its items list, but this might still be useful if
        // we switched to ListView-based selection instead
        //
        //public IEnumerable<string> EventImportModes
        //{
        //    get { return System.Enum.GetValues(typeof(EventImportMode)).Cast<string>(); }
        //}

        public IList<Event> EventsToImport { get; private set; }

        public ICommand DoneCommand { get { return _doneCommand; } }

        #endregion

        #region Constructor

        public NewItineraryViewModel()
        {
            _doneCommand = new Command(Done, () => !string.IsNullOrWhiteSpace(_name));
        }

        #endregion

        #region Private Methods

        protected override async void Done()
        {
            try
            {
                if (_importMode == EventImportMode.Itinerary)
                {
                    var selectItineraryVM = await Navigator.PushModalAndWaitAsync<SelectItineraryViewModel>(vm => vm.Trip = Trip);

                    if (selectItineraryVM.Result != ModalResult.Canceled)
                    {
                        EventsToImport = await DataStore.GetEvents(selectItineraryVM.SelectedItinerary);

                        foreach (var ev in EventsToImport)
                        {
                            ev.ID = 0;
                            ev.ItineraryID = 0;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
                else if (_importMode == EventImportMode.Calendar)
                {
                    var selectCalendarVM = await Navigator.PushModalAndWaitAsync<SelectCalendarViewModel>();

                    if (selectCalendarVM.Result != ModalResult.Canceled)
                    {
                        EventsToImport = await CalendarService.GetEventsAsync(selectCalendarVM.SelectedCalendar, Trip.Start, Trip.End);
                    }
                    else
                    {
                        return;
                    }
                }

                Result = ModalResult.Done;

                _itinerary = new Itinerary
                {
                    Name = _name,
                    TripID = Trip.ID
                };

                await Navigator.PopModalAsync();
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        #endregion
    }

    public enum EventImportMode
    {
        None,
        Itinerary,
        Calendar
    }
}
