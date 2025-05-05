using Tutorial_07.Entities;

namespace Tutorial_07.Services.Abstract;

public interface ITripService
{
    Task<IEnumerable<Trip>> GetTripsAsync();
    Task<IEnumerable<Trip>> GetTripsForClientAsync(int clientId);
    Task<bool> RegisterClientForTripAsync(int clientId, int tripId);
    Task<bool> UnregisterClientFromTripAsync(int clientId, int tripId);
}
