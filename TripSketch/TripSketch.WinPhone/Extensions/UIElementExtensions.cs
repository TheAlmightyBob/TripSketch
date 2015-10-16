using System.Windows;

namespace TripSketch.WinPhone.Extensions
{
    public static class UIElementExtensions
    {
        public static Point GetRelativePosition(this UIElement element, UIElement relativeTo = null)
        {
            if (element == null) return default(Point);

            if (relativeTo == null)
            {
                relativeTo = Application.Current.RootVisual;
            }

            return element.TransformToVisual(relativeTo).Transform(new Point(0, 0));
        }
    }
}
