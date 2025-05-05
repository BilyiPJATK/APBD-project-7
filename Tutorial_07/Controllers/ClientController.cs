using Microsoft.AspNetCore.Mvc;
using Tutorial_07.Entities;
using Tutorial_07.Services;
using Tutorial_07.Services.Abstract;

namespace Tutorial_07.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;

    public ClientController(IClientService clientService)
    {
        _clientService = clientService;
    }

    [HttpGet]
    public async Task<IActionResult> GetClients()
    {
        var clients = await _clientService.GetClientsAsync();
        return Ok(clients);
    }

    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] Client client)
    {
        await _clientService.CreateClientAsync(client);
        return CreatedAtAction(nameof(GetClients), new { id = client.Id }, client);
    }

    [HttpDelete("{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var deleted = await _clientService.DeleteClientAsync(idClient);

        if (!deleted)
        {
            return BadRequest("Client cannot be deleted because they are assigned to a trip.");
        }

        return NoContent();
    }
}