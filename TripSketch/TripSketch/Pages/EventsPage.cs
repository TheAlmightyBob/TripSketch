using TripSketch.Controls;
using TripSketch.Enums;
using TripSketch.Views;
using Xamarin.Forms;

namespace TripSketch.Pages
{
    public class EventsPage : RotatingPage<EventsView, EventsGridView> // EventsViewGrid>
    {
        public EventsPage()
        {
            var saveItem = new ToolbarItemEx { Text = "Export" };
            saveItem.SetBinding(ToolbarItem.CommandProperty, new Binding("SaveEventsCommand"));
            if (Device.OS == TargetPlatform.WinPhone)
            {
                saveItem.Icon = new FileImageSource { File = "Toolkit.Content/ApplicationBar.Select.png" };
            }
            ToolbarItems.Add(saveItem);

            var addItem = new ToolbarItemEx { Text = "Add", ToolbarItemType = ToolbarItemType.Add };
            addItem.SetBinding(ToolbarItem.CommandProperty, new Binding("AddEventCommand"));
            if (Device.OS == TargetPlatform.WinPhone)
            {
                addItem.Icon = new FileImageSource { File = "Toolkit.Content/ApplicationBar.Add.png" };
            }
            ToolbarItems.Add(addItem);

            SetBinding(ContentPage.TitleProperty, new Binding("Title"));
        }

        
    }
}
