using System;
using Xamarin.Forms;

namespace TripSketch.Events
{
    public class TapEventArgs : EventArgs
    {
        /// <summary>
        /// Tap coordinate relative to the view that fired the event
        /// </summary>
        public Point Point { get; private set; }

        public TapEventArgs(Point point)
        {
            Point = point;
        }
    }
}
