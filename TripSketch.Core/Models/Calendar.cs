
namespace TripSketch.Core.Models
{
    public class Calendar
    {
        public string Name { get; set; }

        // Each platform has *some* type of unique identifier besides name
        public string ExternalID { get; set; }
    }
}
