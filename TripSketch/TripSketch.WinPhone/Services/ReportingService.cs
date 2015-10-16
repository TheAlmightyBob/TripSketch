using System;
using System.Windows;
using TripSketch.Core.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(TripSketch.WinPhone.Services.ReportingService))]

namespace TripSketch.WinPhone.Services
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
                    MessageBox.Show(details, message, MessageBoxButton.OK);
                });
        }
    }
}
