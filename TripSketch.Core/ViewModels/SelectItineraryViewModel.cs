using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using TripSketch.Core.Models;
using TripSketch.Core.Enums;

namespace TripSketch.Core.ViewModels
{
    public class SelectItineraryViewModel : ModalViewModelBase
    {
        #region Fields

        private ObservableCollection<Itinerary> _itineraries;
        private Itinerary _selectedItinerary;

        #endregion

        #region Properties

        public string Title { get { return "Select Itinerary"; } }

        public Trip Trip { get; set; }

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

        public Itinerary SelectedItinerary
        {
            get { return _selectedItinerary; }
            set
            {
                if (_selectedItinerary != value)
                {
                    _selectedItinerary = value;
                    HasChanged();

                    Result = ModalResult.Done;
                    Navigator.PopModalAsync();
                }
            }
        }

        #endregion

        public override void Initialize()
        {
            FetchItineraries();
        }

        private async void FetchItineraries()
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
    }
}
