using System;
using TripSketch.Events;
using Xamarin.Forms;

namespace TripSketch.Controls
{
    public class DraggableView : ContentView
    {
        public event EventHandler DragStart;
        public event EventHandler<DragEventArgs> Dragged;
        public event EventHandler<DragEventArgs> Dragging;
        public event EventHandler DragCanceled;

        public void OnDragStart()
        {
            if (DragStart != null)
            {
                DragStart(this, EventArgs.Empty);
            }
        }

        public void OnDragged(Point center)
        {
            if (Dragged != null)
            {
                Dragged(this, new DragEventArgs(center));
            }
        }

        public void OnDragging(Point center)
        {
            if (Dragging != null)
            {
                Dragging(this, new DragEventArgs(center));
            }
        }

        public void OnDragCanceled()
        {
            if (DragCanceled != null)
            {
                DragCanceled(this, EventArgs.Empty);
            }
        }
    }
}
