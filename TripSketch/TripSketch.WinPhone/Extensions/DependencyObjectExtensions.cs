using System.Windows;
using System.Windows.Media;

namespace TripSketch.WinPhone.Extensions
{
    public static class DependencyObjectExtensions
    {
        public static T GetParentOfType<T>(this DependencyObject element) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(element);
            if (parent is T)
            {
                return (T)parent;
            }
            if (parent == null)
            {
                return null;
            }

            return GetParentOfType<T>(parent);
        }

        public static T GetTopmostParentOfType<T>(this DependencyObject element) where T : DependencyObject
        {
            T temp = element.GetParentOfType<T>();
            T result = temp;

            if (temp != null)
            {
                while ((temp = temp.GetParentOfType<T>()) != null)
                {
                    result = temp;
                }
            }

            return result;
        }
    }
}
