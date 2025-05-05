using Microsoft.Data.SqlClient;
using Tutorial_07.Entities;
using Tutorial_07.Services.Abstract;

namespace Tutorial_07.Services;


public class TripService : ITripService
    {
        private readonly IConfiguration _configuration;

        public TripService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<IEnumerable<Trip>> GetTripsAsync()
        {
            var trips = new List<Trip>();
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var command = new SqlCommand(@"
                SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
                       c.IdCountry, c.Name AS CountryName
                FROM Trip t
                LEFT JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
                LEFT JOIN Country c ON ct.IdCountry = c.IdCountry", connection);

            await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            var tripDict = new Dictionary<int, Trip>();
            while (await reader.ReadAsync())
            {
                var idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                if (!tripDict.TryGetValue(idTrip, out var trip))
                {
                    trip = new Trip
                    {
                        Id = idTrip,
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        Description = reader.GetString(reader.GetOrdinal("Description")),
                        DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                        DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                        MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                        Countries = new List<Country>()
                    };
                    tripDict.Add(idTrip, trip);
                }

                if (!reader.IsDBNull(reader.GetOrdinal("IdCountry")))
                {
                    trip.Countries.Add(new Country
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("IdCountry")),
                        Name = reader.GetString(reader.GetOrdinal("CountryName"))
                    });
                }
            }

            return tripDict.Values;
        }

        public async Task<IEnumerable<Trip>> GetTripsForClientAsync(int clientId)
{
    var trips = new List<Trip>();
    using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    using var command = new SqlCommand(@"
        SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
               c.IdCountry, c.Name AS CountryName
        FROM Trip t
        INNER JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
        INNER JOIN Country_Trip ctp ON t.IdTrip = ctp.IdTrip
        LEFT JOIN Country c ON ctp.IdCountry = c.IdCountry
        WHERE ct.IdClient = @ClientId", connection);

    command.Parameters.AddWithValue("@ClientId", clientId);

    try
    {
        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        var tripDict = new Dictionary<int, Trip>();

        while (await reader.ReadAsync())
        {
            var idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
            if (!tripDict.TryGetValue(idTrip, out var trip))
            {
                trip = new Trip
                {
                    Id = idTrip,
                    Name = reader.GetString(reader.GetOrdinal("Name")),
                    Description = reader.GetString(reader.GetOrdinal("Description")),
                    DateFrom = reader.GetDateTime(reader.GetOrdinal("DateFrom")),
                    DateTo = reader.GetDateTime(reader.GetOrdinal("DateTo")),
                    MaxPeople = reader.GetInt32(reader.GetOrdinal("MaxPeople")),
                    Countries = new List<Country>()
                };
                tripDict.Add(idTrip, trip);
            }

            if (!reader.IsDBNull(reader.GetOrdinal("IdCountry")))
            {
                trip.Countries.Add(new Country
                {
                    Id = reader.GetInt32(reader.GetOrdinal("IdCountry")),
                    Name = reader.GetString(reader.GetOrdinal("CountryName"))
                });
            }
        }

        return tripDict.Values; // Now returning IEnumerable<Trip>
    }
    catch (Exception ex)
    {
        throw new Exception($"Error retrieving trips for client {clientId}: {ex.Message}");
    }
}


        public async Task<bool> RegisterClientForTripAsync(int clientId, int tripId)
{
    using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
    await connection.OpenAsync();

    using var transaction = connection.BeginTransaction();

    try
    {
        
        var clientCheck = new SqlCommand("SELECT 1 FROM Client WHERE IdClient = @IdClient", connection, transaction);
        clientCheck.Parameters.AddWithValue("@IdClient", clientId);
        if (await clientCheck.ExecuteScalarAsync() == null)
        {
            Console.WriteLine($"Client with ID {clientId} not found.");
            return false;
        }

        
        var tripCheck = new SqlCommand("SELECT MaxPeople FROM Trip WHERE IdTrip = @IdTrip", connection, transaction);
        tripCheck.Parameters.AddWithValue("@IdTrip", tripId);
        var maxPeopleObj = await tripCheck.ExecuteScalarAsync();
        if (maxPeopleObj == null)
        {
            Console.WriteLine($"Trip with ID {tripId} not found.");
            return false;
        }
        int maxPeople = (int)maxPeopleObj;

        var countCmd = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip", connection, transaction);
        countCmd.Parameters.AddWithValue("@IdTrip", tripId);
        int currentParticipants = (int)(await countCmd.ExecuteScalarAsync() ?? 0);
        if (currentParticipants >= maxPeople)
        {
            Console.WriteLine($"Trip with ID {tripId} is full. Max capacity reached.");
            return false;
        }

       
        var existingCheck = new SqlCommand("SELECT 1 FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", connection, transaction);
        existingCheck.Parameters.AddWithValue("@IdClient", clientId);
        existingCheck.Parameters.AddWithValue("@IdTrip", tripId);
        if (await existingCheck.ExecuteScalarAsync() != null)
        {
            Console.WriteLine($"Client with ID {clientId} is already registered for trip {tripId}.");
            return false;
        }

       
        var insertCmd = new SqlCommand(@"
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@IdClient, @IdTrip, @RegisteredAt)", connection, transaction);
        insertCmd.Parameters.AddWithValue("@IdClient", clientId);
        insertCmd.Parameters.AddWithValue("@IdTrip", tripId);
        insertCmd.Parameters.AddWithValue("@RegisteredAt", DateTime.UtcNow);

        await insertCmd.ExecuteNonQueryAsync();
        await transaction.CommitAsync();
        Console.WriteLine($"Client {clientId} successfully registered for trip {tripId}.");
        return true;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        Console.WriteLine($"Error: {ex.Message}");
        return false;
    }
}



        public async Task<bool> UnregisterClientFromTripAsync(int clientId, int tripId)
        {
            using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            var command = new SqlCommand("DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip", connection);
            command.Parameters.AddWithValue("@IdClient", clientId);
            command.Parameters.AddWithValue("@IdTrip", tripId);

            await connection.OpenAsync();
            int affected = await command.ExecuteNonQueryAsync();
            return affected > 0;
        }
    }