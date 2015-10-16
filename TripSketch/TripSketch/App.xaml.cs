using TripSketch.Core;
using TripSketch.Core.ViewModels;
using TripSketch.Core.Services;
using TripSketch.Core.Helpers;
using TripSketch.Helpers;
using TripSketch.Pages;
using Xamarin.Forms;

namespace TripSketch
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            DependencyService.Register<ViewProvider>();
            DependencyService.Register<IDataStore, DataStore>();

            RegisterViews();

            var viewProvider = DependencyService.Get<ViewProvider>();

            MainPage = new NavigationPage(viewProvider.GetView(ViewModelProvider.GetViewModel<TripsViewModel>()) as Page);
        }

        private void RegisterViews()
        {
            var viewProvider = DependencyService.Get<ViewProvider>();

            viewProvider.Register<NewCalendarViewModel, NewCalendarPage>();
            viewProvider.Register<WriteableCalendarsViewModel, WriteableCalendarsPage>();
            viewProvider.Register<EventEditorViewModel, EventEditorPage>();

            viewProvider.Register<TripsViewModel, TripsPage>();
            viewProvider.Register<NewTripViewModel, NewTripPage>();
            viewProvider.Register<ItinerariesViewModel, ItinerariesPage>();
            viewProvider.Register<ItineraryViewModel, EventsPage>();
            viewProvider.Register<NewItineraryViewModel, NewItineraryPage>();
            viewProvider.Register<SelectCalendarViewModel, SelectCalendarPage>();
            viewProvider.Register<SelectItineraryViewModel, SelectItineraryPage>();
        }
    }
}