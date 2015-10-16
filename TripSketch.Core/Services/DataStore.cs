using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TripSketch.Core.Models;

namespace TripSketch.Core.Services
{
    public class DataStore : IDataStore
    {
        const string location = "tripsketch.db3";
        public static string Root { get; set; }
        SQLiteAsyncConnection Connection { get; }

        public DataStore()
        {
            Connection = new SQLiteAsyncConnection(Path.Combine(Root, location));

            //create tables

            Connection.CreateTableAsync<Trip>().Wait();
            Connection.CreateTableAsync<Itinerary>().Wait();
            Connection.CreateTableAsync<Event>().Wait();
        }

        public Task AddEvent(Event ev)
        {
            return Connection.InsertAsync(ev);
        }

        public Task AddItinerary(Itinerary itinerary)
        {
            return Connection.InsertAsync(itinerary);
        }

        public Task AddTrip(Trip trip)
        {
            return Connection.InsertAsync(trip);
        }

        public async Task<IList<Event>> GetEvents(Itinerary itinerary)
        {
            return await Connection.Table<Event>().Where(i => i.ItineraryID == itinerary.ID).ToListAsync().ConfigureAwait(false);
        }

        public async Task<IList<Itinerary>> GetItineraries(Trip trip)
        {
            return await Connection.Table<Itinerary>().Where(i => i.TripID == trip.ID).ToListAsync().ConfigureAwait(false);
        }

        public async Task<IList<Trip>> GetTrips()
        {
            return await Connection.Table<Trip>().ToListAsync().ConfigureAwait(false);
        }

        public Task RemoveEvent(Event ev)
        {
            return Connection.DeleteAsync(ev);
        }

        public Task RemoveItinerary(Itinerary itinerary)
        {
            return Connection.DeleteAsync(itinerary);
        }

        public Task RemoveTrip(Trip trip)
        {
            return Connection.DeleteAsync(trip);
        }

        public Task UpdateEvent(Event ev)
        {
            return Connection.UpdateAsync(ev);
        }

        public Task UpdateItinerary(Itinerary itinerary)
        {
            return Connection.UpdateAsync(itinerary);
        }

        public Task UpdateTrip(Trip trip)
        {
            return Connection.UpdateAsync(trip);
        }
    }
}
