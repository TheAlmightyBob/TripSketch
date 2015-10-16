using System;
using TripSketch.Core.Services;

namespace TripSketch.Mocks
{
    public class MockReportingService : IReportingService
    {
        public Action<Exception> ReportExceptionImpl { get; set; }
        public Action<string, string> ReportMessageImpl { get; set; }

        public void ReportException(Exception ex)
        {
            if (ReportExceptionImpl != null)
            {
                ReportExceptionImpl(ex);
            }
            else
            {
                // Well this is awkward.
                throw new NotImplementedException();
            }
        }

        public void ReportMessage(string message, string details)
        {
            if (ReportMessageImpl != null)
            {
                ReportMessageImpl(message, details);
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}
