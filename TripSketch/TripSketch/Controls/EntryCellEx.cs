using System;
using Xamarin.Forms;

namespace TripSketch.Controls
{
    public class EntryCellEx : EntryCell
    {
        public void Focus()
        {
            if (FocusRequested != null)
            {
                FocusRequested(this, EventArgs.Empty);
            }
        }

        public event EventHandler FocusRequested;

        // A more "proper" method would be to add support for EntryCellEx to InitialFocusBehavior...
        // ...but this is easier.
        //
        public bool RequestInitialFocus { get; set; }
    }
}
