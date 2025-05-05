using Microsoft.Data.SqlClient;
using Tutorial_07.Entities;
using Tutorial_07.Services.Abstract;

namespace Tutorial_07.Services;

public class ClientService : IClientService
{
    private readonly IConfiguration _configuration;

    public ClientService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<IEnumerable<Client>> GetClientsAsync()
    {
        var clients = new List<Client>();

        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var command = new SqlCommand("SELECT IdClient, FirstName, LastName, Email, Telephone, Pesel FROM Client", connection);

        await connection.OpenAsync();
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            clients.Add(new Client
            {
                Id = reader.GetInt32(reader.GetOrdinal("IdClient")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Telephone = reader.GetString(reader.GetOrdinal("Telephone")),
                Pesel = reader.GetString(reader.GetOrdinal("Pesel"))
            });
        }

        return clients;
    }

    public async Task CreateClientAsync(Client client)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        using var command = new SqlCommand(@"
            INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
            VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel)", connection);

        command.Parameters.AddWithValue("@FirstName", client.FirstName);
        command.Parameters.AddWithValue("@LastName", client.LastName);
        command.Parameters.AddWithValue("@Email", client.Email);
        command.Parameters.AddWithValue("@Telephone", client.Telephone);
        command.Parameters.AddWithValue("@Pesel", client.Pesel);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> DeleteClientAsync(int idClient)
    {
        using var connection = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        await connection.OpenAsync();

        // Check if client has any trips
        using var checkCommand = new SqlCommand("SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @IdClient", connection);
        checkCommand.Parameters.AddWithValue("@IdClient", idClient);
        var count = (int)await checkCommand.ExecuteScalarAsync();

        if (count > 0)
            return false; // Cannot delete

        // Safe to delete
        using var deleteCommand = new SqlCommand("DELETE FROM Client WHERE IdClient = @IdClient", connection);
        deleteCommand.Parameters.AddWithValue("@IdClient", idClient);
        var rowsAffected = await deleteCommand.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }
}