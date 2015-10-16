using System;
using TripSketch.Core.Helpers;
using TripSketch.Core.Services;

namespace TripSketch.Core.ViewModels
{
    public class ViewModelBase : PropertyChangeNotifier
    {
        #region Dependencies

        public IDataStore DataStore { get; set; }
        public ICalendarService CalendarService { get; set; }
        public IReportingService ReportingService { get; set; }
        public INavigator Navigator { get; set; }

        #endregion

        public virtual void Initialize() { }


        protected void ReportError(Exception ex)
        {
            if (ReportingService != null)
            {
                ReportingService.ReportException(ex);
            }
        }

        protected void ReportMessage(string message, string details)
        {
            if (ReportingService != null)
            {
                ReportingService.ReportMessage(message, details);
            }
        }
    }
}
