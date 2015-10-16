using System;
using TripSketch.Events;
using Xamarin.Forms;

namespace TripSketch.Controls
{
    /// <summary>
    /// A simple view with a Tapped event that provides the coordinates of the tap
    /// within the view (because TapGestureRecognizer does not).
    /// </summary>
    public class TappableView : ContentView
    {
        public event EventHandler<TapEventArgs> Tapped;

        public void OnTap(Point point)
        {
            if (Tapped != null)
            {
                Tapped(this, new TapEventArgs(point));
            }
        }
    }
}
