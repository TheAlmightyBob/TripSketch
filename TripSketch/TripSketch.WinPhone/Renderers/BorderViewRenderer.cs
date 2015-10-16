using System.Windows.Controls;
using System.Windows.Media;
using TripSketch.Controls;
using TripSketch.WinPhone.Renderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinPhone;

[assembly: ExportRenderer(typeof(BorderView), typeof(BorderViewRenderer))]

namespace TripSketch.WinPhone.Renderers
{
    public class BorderViewRenderer : ViewRenderer<BorderView, Border>
    {
        protected override void OnElementChanged(ElementChangedEventArgs<BorderView> e)
        {
            base.OnElementChanged(e);

            if (Control == null)
            {
                SetNativeControl(new Border());
            }

            if (e.NewElement != null)
            {
                UpdateBorder();
            }
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e.PropertyName == BorderView.BorderColorProperty.PropertyName ||
                e.PropertyName == BorderView.BorderThicknessProperty.PropertyName)
            {
                UpdateBorder();
            }
        }

        private void UpdateBorder()
        {
            var converter = new ColorConverter();
            Control.BorderBrush = (SolidColorBrush)converter.Convert(Element.BorderColor, null, null, null);
            Control.BorderThickness = new System.Windows.Thickness(Element.BorderThickness);
        }
    }
}
