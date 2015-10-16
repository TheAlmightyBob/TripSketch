using System;
using TripSketch.Core.Services;
using TripSketch.Core.ViewModels;
using Xamarin.Forms;

namespace TripSketch.Core.Helpers
{
    public static class ViewModelProvider
    {
        public static TViewModel GetViewModel<TViewModel>(Action<TViewModel> customInit = null) where TViewModel : ViewModelBase, new()
        {
            var vm = new TViewModel();

            // Satisfy dependencies
            vm.CalendarService = new CalendarService(); // DependencyService.Get<ICalendarService>();
            vm.ReportingService = DependencyService.Get<IReportingService>();
            vm.DataStore = DependencyService.Get<IDataStore>();

            // Allow the caller to include some pre-Initialize configuration
            //
            if (customInit != null)
            {
                customInit(vm);
            }

            vm.Initialize();

            return vm;
        }
    }
}
