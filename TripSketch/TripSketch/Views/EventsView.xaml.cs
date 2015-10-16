
using System;
using TripSketch.Core.Models;
using TripSketch.Core.ViewModels;
using Xamarin.Forms;
namespace TripSketch.Views
{
    public partial class EventsView
    {
        public EventsView()
        {
            InitializeComponent();
        }

        //private void Events_ItemTapped(object sender, ItemTappedEventArgs e)
        //{
        //    var vm = BindingContext as EventsViewModel;

        //    if (vm != null && vm.EditEventCommand != null && vm.EditEventCommand.CanExecute(e.Item))
        //    {
        //        vm.EditEventCommand.Execute(e.Item);
        //    }

        //    // As of Xamarin.Forms 1.3, ItemTapped no longer fires on WinPhone
        //    // for already-selected items
        //    // (known bug, fix planned for 1.3.2)
        //    //
        //    //Events.ClearValue(ListView.SelectedItemProperty);
        //}

        //private void OnDelete(object sender, EventArgs e)
        //{
        //    var vm = BindingContext as EventsViewModel;
        //    var menuItem = sender as MenuItem;
        //    var ev = menuItem.BindingContext as Event;

        //    if (vm != null && menuItem != null && ev != null
        //        && vm.DeleteEventCommand != null && vm.DeleteEventCommand.CanExecute(ev))
        //    {
        //        vm.DeleteEventCommand.Execute(ev);
        //    }
        //}
    }
}
