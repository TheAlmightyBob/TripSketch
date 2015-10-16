using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripSketch.Core.Helpers;
using TripSketch.Core.ViewModels;
using TripSketch.Helpers;
using Xamarin.Forms;

namespace TripSketch.Pages
{
    public partial class TripsPage : ContentPage
    {
        public TripsPage()
        {
            InitializeComponent();

            //BindingContext = ViewModelProvider.GetViewModel<TripsViewModel>(vm => vm.Navigator = new Navigator(this));

            //var picker = new DatePicker();
        }
    }
}
