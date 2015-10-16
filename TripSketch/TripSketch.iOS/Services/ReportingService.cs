using UIKit;
using System;
using TripSketch.Core.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(TripSketch.iOS.Services.ReportingService))]

namespace TripSketch.iOS.Services
{
    public class ReportingService : IReportingService
    {
        public void ReportException(Exception ex)
        {
            ReportMessage("Error", ex.Message);
        }

        public void ReportMessage(string message, string details)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                new UIAlertView(message, details, null, "Righto", null).Show();
            });
        }
    }
}
