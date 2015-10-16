using TripSketch.Core.Helpers;
using TripSketch.Core.ViewModels;
using TripSketch.Helpers;
using Xamarin.Forms;

namespace TripSketch.Pages
{
    public partial class NewCalendarPage : ContentPage
    {
        public NewCalendarPage()
        {
            InitializeComponent();

            BindingContext = ViewModelProvider.GetViewModel<NewCalendarViewModel>(vm => vm.Navigator = new Navigator(this));
        }
    }
}
