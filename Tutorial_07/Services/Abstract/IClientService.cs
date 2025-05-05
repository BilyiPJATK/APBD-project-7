using Tutorial_07.Entities;

namespace Tutorial_07.Services.Abstract;

public interface IClientService
{
    Task<IEnumerable<Client>> GetClientsAsync();
    Task CreateClientAsync(Client client);
    Task<bool> DeleteClientAsync(int id);
}