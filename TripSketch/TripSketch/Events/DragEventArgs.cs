using System;
using Xamarin.Forms;

namespace TripSketch.Events
{
    public class DragEventArgs : EventArgs
    {
        public Point Center { get; private set; }

        public DragEventArgs(Point center)
        {
            Center = center;
        }
    }
}
