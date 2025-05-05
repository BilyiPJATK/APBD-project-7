using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Tutorial_07.Entities;
using Tutorial_07.Services.Abstract;

namespace Tutorial_07.Controllers;

 [ApiController]
    [Route("api/trips")]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            try
            {
                var trips = await _tripService.GetTripsAsync();
                return Ok(trips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("client/{clientId}")]
        public async Task<IActionResult> GetTripsForClient(int clientId)
        {
            try
            {
                var trips = await _tripService.GetTripsForClientAsync(clientId);
                if (trips == null || !trips.Any())
                {
                    return NotFound($"No trips found for client with ID {clientId}");
                }
                return Ok(trips);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterClientForTrip([FromBody] RegisterTripRequest request)
        {
            try
            {
                var result = await _tripService.RegisterClientForTripAsync(request.ClientId, request.TripId);
                if (result)
                {
                    return Ok($"Client {request.ClientId} successfully registered for trip {request.TripId}");
                }
                else
                {
                    return BadRequest($"Failed to register client {request.ClientId} for trip {request.TripId}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("unregister")]
        public async Task<IActionResult> UnregisterClientFromTrip([FromBody] RegisterTripRequest request)
        {
            try
            {
                var result = await _tripService.UnregisterClientFromTripAsync(request.ClientId, request.TripId);
                if (result)
                {
                    return Ok($"Client {request.ClientId} successfully unregistered from trip {request.TripId}");
                }
                else
                {
                    return BadRequest($"Failed to unregister client {request.ClientId} from trip {request.TripId}");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("api/clients/{clientId}/trips")]
        public async Task<IActionResult> GetTripsForClientREST(int clientId)
        {
            var trips = await _tripService.GetTripsForClientAsync(clientId);
            if (trips == null || !trips.Any())
            {
                return NotFound($"No trips found for client with ID {clientId}");
            }
            return Ok(trips);
        }

        [HttpPut("api/clients/{clientId}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClientForTrip(int clientId, int tripId)
        {
            var result = await _tripService.RegisterClientForTripAsync(clientId, tripId);
            if (result)
            {
                return Ok("Successfully registered for the trip");
            }
            return BadRequest("Failed to register client for trip");
        }
        
        [HttpDelete("api/clients/{clientId}/trips/{tripId}")]
        public async Task<IActionResult> UnregisterClientFromTrip(int clientId, int tripId)
        {
            var result = await _tripService.UnregisterClientFromTripAsync(clientId, tripId);
            if (result)
            {
                return Ok("Successfully unregistered from the trip");
            }
            return BadRequest("Failed to unregister client from the trip");
        }





    }

    public class RegisterTripRequest
    {
        public int ClientId { get; set; }
        public int TripId { get; set; }
    }