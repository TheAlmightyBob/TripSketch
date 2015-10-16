using System;

namespace TripSketch.Core.Services
{
    public interface IReportingService
    {
        void ReportException(Exception ex);
        void ReportMessage(string message, string details);
    }
}
