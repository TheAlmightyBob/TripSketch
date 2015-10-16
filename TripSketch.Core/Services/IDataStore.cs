using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TripSketch.Core.Models;

namespace TripSketch.Core.Services
{
    public interface IDataStore
    {
        Task<IList<Trip>> GetTrips();
        Task AddTrip(Trip trip);
        Task UpdateTrip(Trip trip);
        Task RemoveTrip(Trip trip);

        Task<IList<Itinerary>> GetItineraries(Trip trip);
        Task AddItinerary(Itinerary itinerary);
        Task UpdateItinerary(Itinerary itinerary);
        Task RemoveItinerary(Itinerary itinerary);

        Task<IList<Event>> GetEvents(Itinerary itinerary);
        Task AddEvent(Event ev);
        Task UpdateEvent(Event ev);
        Task RemoveEvent(Event ev);

    }
}
