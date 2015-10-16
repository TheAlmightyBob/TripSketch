using TripSketch.Controls;
using TripSketch.WinPhone.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(EntryCellEx), typeof(EntryCellRendererEx))]

namespace TripSketch.WinPhone.Renderers
{
    public class EntryCellRendererEx : EntryCellRenderer
    {
        public EntryCellRendererEx()
        {
        }

        public override System.Windows.DataTemplate GetTemplate(Cell cell)
        {
            return (System.Windows.DataTemplate)System.Windows.Application.Current.Resources["EntryCellEx"];
        }
    }
}
