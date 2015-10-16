using Xamarin.Forms;

namespace TripSketch.Controls
{
    public class BorderView : ContentView
    {
        // Bindable property for the border color
        public static readonly BindableProperty BorderColorProperty =
          BindableProperty.Create<BorderView, Color>(p => p.BorderColor, Color.Default);

        //Gets or sets the color of the border
        public Color BorderColor
        {
            get { return (Color)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        // Bindable property for the border thickness
        public static readonly BindableProperty BorderThicknessProperty =
          BindableProperty.Create<BorderView, double>(p => p.BorderThickness, 1.0);

        // Gets or sets the border thickness
        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }
    }
}
