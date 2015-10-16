using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using TripSketch.Core.Enums;
using TripSketch.Core.Models;
using Xamarin.Forms;

namespace TripSketch.Core.ViewModels
{
    public class ItineraryViewModel : ViewModelBase
    {
        #region Fields

        private ObservableCollection<Event> _events;
        private Event _selectedEvent;

        private ObservableCollection<EventsForDay> _eventsByDay;

        private Event _editingEvent;

        #endregion

        #region Properties

        public string Title { get { return "Events"; } }

        public Trip Trip { get; set; }
        public Itinerary Itinerary { get; set; }

        public ObservableCollection<Event> Events
        {
            get { return _events; }
            set
            {
                if (_events != value)
                {
                    if (_events != null)
                    {
                        foreach (var ev in _events)
                        {
                            ev.PropertyChanged -= Event_PropertyChanged;
                        }
                        _events.CollectionChanged -= Events_CollectionChanged;
                    }

                    _events = value;

                    if (_events != null)
                    {
                        foreach (var ev in _events)
                        {
                            ev.PropertyChanged += Event_PropertyChanged;
                        }
                        _events.CollectionChanged += Events_CollectionChanged;
                    }

                    HasChanged();
                }
            }
        }

        public Event SelectedEvent
        {
            get { return _selectedEvent; }
            set
            {
                if (_selectedEvent != value)
                {
                    _selectedEvent = value;
                    HasChanged();
                }
            }
        }

        public ObservableCollection<EventsForDay> EventsByDay
        {
            get { return _eventsByDay; }
            set
            {
                if (_eventsByDay != value)
                {
                    _eventsByDay = value;
                    HasChanged();
                }
            }
        }

        public DateTime Start
        {
            get { return Trip.Start; }
        }

        public DateTime End
        {
            get { return Trip.End; }
        }

        public ICommand SaveEventsCommand { get { return new Command(ExportEventsToSystemCalendar); } }
        public ICommand AddEventCommand { get { return new Command<DateTime?>(AddEvent); } }
        public ICommand EditEventCommand { get { return new Command<Event>(EditEvent); } }
        public ICommand DeleteEventCommand { get { return new Command<Event>(DeleteEvent); } }

        #endregion

        #region Overrides

        public override void Initialize()
        {
            FetchEvents();
        }

        #endregion

        #region Private Methods

        private async void FetchEvents()
        {
            try
            {
                Events = new ObservableCollection<Event>(await DataStore.GetEvents(Itinerary));

                GroupEventsByDay();
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private async void ExportEventsToSystemCalendar()
        {
            Debug.Assert(CalendarService != null);

            if (CalendarService != null)
            {
                try
                {
                    var writeableCalendars = await CalendarService.GetCalendarsAsync(true);
                    Calendar destinationCalendar = null;

                    // If there are no writeable calendars, skip straight to creation page
                    //
                    if (!writeableCalendars.Any())
                    {
                        var newCalVM = await Navigator.PushModalAndWaitAsync<NewCalendarViewModel>();

                        if (newCalVM.Result == ModalResult.Canceled)
                        {
                            destinationCalendar = null;
                            ReportMessage("Save canceled", "");
                        }
                        else
                        {
                            destinationCalendar = new Calendar { Name = newCalVM.CalendarName };
                        }
                    }
                    else
                    {
                        // Select a destination calendar
                        //
                        var selectCalVM = await Navigator.PushModalAndWaitAsync<WriteableCalendarsViewModel>(vm => vm.Calendars = writeableCalendars);

                        if (selectCalVM.Result == ModalResult.Canceled)
                        {
                            destinationCalendar = null;
                            ReportMessage("Save canceled", "");
                        }
                        else
                        {
                            destinationCalendar = selectCalVM.SelectedCalendar;
                        }
                    }

                    if (destinationCalendar != null)
                    {
                        await CalendarService.ExportEventsAsync(destinationCalendar, Events);

                        ReportMessage("Save successful", "");
                    }
                }
                catch (Exception ex)
                {
                    ReportMessage("Save failed", ex.Message);
                }
            }
        }

        private async void AddEvent(DateTime? startDate = null)
        {
            try
            {
                var eventVM = await Navigator.PushModalAndWaitAsync<EventEditorViewModel>(vm =>
                {
                    if (startDate.HasValue)
                    {
                        vm.Start = startDate.Value;
                    }
                });

                if (eventVM.Result != ModalResult.Canceled)
                {
                    eventVM.Event.ItineraryID = Itinerary.ID;
                    await DataStore.AddEvent(eventVM.Event);
                    Events.Add(eventVM.Event);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private async void EditEvent(Event ev)
        {
            try
            {
                _editingEvent = ev;

                var eventVM = await Navigator.PushModalAndWaitAsync<EventEditorViewModel>(vm => vm.Event = ev);

                if (eventVM.Result == ModalResult.Deleted)
                {
                    await DataStore.RemoveEvent(ev);
                    Events.Remove(ev);
                }
                else if (eventVM.Result != ModalResult.Canceled)
                {
                    await DataStore.UpdateEvent(ev);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }

            _editingEvent = null;
        }

        private async void DeleteEvent(Event ev)
        {
            try
            {
                await DataStore.RemoveEvent(ev);
                Events.Remove(ev);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private void GroupEventsByDay()
        {
            ObservableCollection<EventsForDay> eventsByDay = new ObservableCollection<EventsForDay>();

            foreach (var ev in Events)
            {
                AddEventToGroup(ev, eventsByDay);
            }

            EventsByDay = eventsByDay;
        }

        #endregion

        #region Static Helpers

        private static void AddEventToGroup(Event ev, ObservableCollection<EventsForDay> eventsByDay)
        {
            for (var date = ev.Start; date < ev.End; date = date.AddDays(1))
            {
                var day = eventsByDay.FirstOrDefault(d => d.Date == date);

                if (day != null)
                {
                    day.Add(ev);
                }
                else
                {
                    day = new EventsForDay { Date = date };
                    day.Add(ev);

                    // Insert in chronological order (rather than following each add with an OrderBy...)
                    //
                    int index = eventsByDay.IndexOf(eventsByDay.FirstOrDefault(e => e.Date > day.Date));
                    if (index < 0)
                    {
                        index = eventsByDay.Count;
                    }
                    eventsByDay.Insert(index, day);
                }
            }
        }

        private static ObservableCollection<EventsForDay> RemoveEventFromGroup(Event ev, ObservableCollection<EventsForDay> eventsByDay)
        {
            foreach (var group in eventsByDay)
            {
                group.Remove(ev);
            }

            return new ObservableCollection<EventsForDay>(eventsByDay.Where(group => group.Count > 0));
        }

        #endregion

        #region Event Handlers

        private void Events_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems.OfType<Event>())
                {
                    item.PropertyChanged -= Event_PropertyChanged;
                    EventsByDay = RemoveEventFromGroup(item, EventsByDay);
                }
            }

            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems.OfType<Event>())
                {
                    item.PropertyChanged += Event_PropertyChanged;
                    AddEventToGroup(item, EventsByDay);
                }
            }
        }

        private async void Event_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var ev = sender as Event;

            if (ev == null)
            {
                return;
            }

            // TODO: Save on every event change? Or something more explicit??
            //       (saving on every change has that ol' issue that if 5 properties change
            //       at once, we wind up saving the same change 5 times due to the separate change events..)

            if (e.PropertyName == "Start" || e.PropertyName == "End")
            {
                _eventsByDay = RemoveEventFromGroup(ev, EventsByDay);
                AddEventToGroup(ev, EventsByDay);
                HasChanged("EventsByDay");
            }

            if (_editingEvent != ev)
            {
                try
                {
                    await DataStore.UpdateEvent(ev);
                }
                catch (Exception ex)
                {
                    ReportError(ex);
                }
            }
        }

        #endregion

    }
}
