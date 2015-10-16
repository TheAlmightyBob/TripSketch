using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using TripSketch.Core.Enums;
using TripSketch.Core.Models;
using Xamarin.Forms;

namespace TripSketch.Core.ViewModels
{
    public class TripsViewModel : ViewModelBase
    {
        #region Fields

        private ObservableCollection<Trip> _trips;

        #endregion

        #region Properties

        public string Title { get { return "Trips"; } }

        public ObservableCollection<Trip> Trips
        {
            get { return _trips; }
            set
            {
                if (_trips != value)
                {
                    _trips = value;
                    HasChanged();
                }
            }
        }

        public ICommand AddCommand { get { return new Command(AddTrip); } }
        public ICommand OpenCommand { get { return new Command<Trip>(OpenTrip); } }
        public ICommand DeleteCommand { get { return new Command<Trip>(DeleteTrip); } }

        #endregion

        public override void Initialize()
        {
            LoadTrips();
        }

        private async void LoadTrips()
        {
            try
            {
                Trips = new ObservableCollection<Trip>(await DataStore.GetTrips());
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private async void AddTrip()
        {
            try
            {
                var newTripVM = await Navigator.PushModalAndWaitAsync<NewTripViewModel>();

                if (newTripVM.Result != ModalResult.Canceled)
                {
                    var trip = newTripVM.Trip;
                    await DataStore.AddTrip(trip);

                    // Initialize trip with a default itinerary
                    await DataStore.AddItinerary(new Itinerary { Name = "Itinerary 1", TripID = trip.ID });

                    Trips.Add(trip);
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private async void OpenTrip(Trip trip)
        {
            try
            {
                await Navigator.PushAsync<ItinerariesViewModel>(vm => vm.Trip = trip);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private async void DeleteTrip(Trip trip)
        {
            try
            {
                await DataStore.RemoveTrip(trip);
                Trips.Remove(trip);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
    }
}
