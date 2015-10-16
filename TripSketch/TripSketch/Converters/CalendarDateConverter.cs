using System;
using System.Globalization;
using Xamarin.Forms;

namespace TripSketch.Converters
{
    public class CalendarDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var date = value as DateTime?;

            if (date == null)
            {
                return null;
            }

            return date.Value.Day == 1 ? date.Value.ToString("MMM d") : date.Value.ToString("%d");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
