using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using TripSketch.Core.Enums;
using TripSketch.Core.Models;
using Xamarin.Forms;

namespace TripSketch.Core.ViewModels
{
    public class ItinerariesViewModel : ViewModelBase
    {
        #region Fields

        private ObservableCollection<Itinerary> _itineraries;

        #endregion

        #region Properties

        public string Title { get { return Trip?.Name + " Itineraries"; } }

        public ObservableCollection<Itinerary> Itineraries
        {
            get { return _itineraries; }
            set
            {
                if (_itineraries != value)
                {
                    _itineraries = value;
                    HasChanged();
                }
            }
        }

        public Trip Trip { get; set; }

        public ICommand AddCommand { get { return new Command(AddItinerary); } }
        public ICommand OpenCommand { get { return new Command<Itinerary>(OpenItinerary); } }
        public ICommand DeleteCommand { get { return new Command<Itinerary>(DeleteItinerary); } }

        #endregion

        public override void Initialize()
        {
            LoadItineraries();
        }

        private async void LoadItineraries()
        {
            try
            {
                Itineraries = new ObservableCollection<Itinerary>(await DataStore.GetItineraries(Trip));
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private async void AddItinerary()
        {
            try
            {
                var newItineraryVM = await Navigator.PushModalAndWaitAsync<NewItineraryViewModel>(vm => vm.Trip = Trip);

                if (newItineraryVM.Result != ModalResult.Canceled)
                {
                    var itinerary = newItineraryVM.Itinerary;
                    await DataStore.AddItinerary(itinerary);
                    Itineraries.Add(itinerary);

                    if (newItineraryVM.EventsToImport != null)
                    {
                        foreach (var ev in newItineraryVM.EventsToImport)
                        {
                            ev.ItineraryID = itinerary.ID;
                            await DataStore.AddEvent(ev);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private async void OpenItinerary(Itinerary itinerary)
        {
            try
            {
                await Navigator.PushAsync<ItineraryViewModel>(vm =>
                {
                    vm.Trip = Trip;
                    vm.Itinerary = itinerary;
                });
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }

        private async void DeleteItinerary(Itinerary itinerary)
        {
            try
            {
                await DataStore.RemoveItinerary(itinerary);
                Itineraries.Remove(itinerary);
            }
            catch (Exception ex)
            {
                ReportError(ex);
            }
        }
    }
}
