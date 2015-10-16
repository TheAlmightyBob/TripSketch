using System;

namespace TripSketch.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Determines if datetime is within given range, with start being inclusive and end being exclusive.
        /// </summary>
        public static bool IsInRange(this DateTime value, DateTime start, DateTime end)
        {
            return start <= value && value <= end;
        }
    }
}
