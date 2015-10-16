using Xamarin.Forms;

namespace TripSketch.Controls
{
    public class TableViewEx : TableView
    {
        // Per-cell control over selectability would be preferable, but harder...
        //
        public static readonly BindableProperty AllowSelectionProperty =
          BindableProperty.Create<TableViewEx, bool>(p => p.AllowSelection, true);

        public bool AllowSelection
        {
            get { return (bool)GetValue(AllowSelectionProperty); }
            set { SetValue(AllowSelectionProperty, value); }
        }
    }
}
