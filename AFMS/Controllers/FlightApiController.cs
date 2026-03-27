using Microsoft.AspNetCore.Mvc;
using AFMS.Models;
using AFMS.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AFMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightApiController : ControllerBase
    {
        private readonly IFlightSearchService _flightSearchService;
        private readonly ILogger<FlightApiController> _logger;

        public FlightApiController(IFlightSearchService flightSearchService, ILogger<FlightApiController> logger)
        {
            _flightSearchService = flightSearchService;
            _logger = logger;
        }

        [HttpGet("search/{flightNumber}")]
        public async Task<ActionResult<IEnumerable<Flight>>> SearchByFlightNumber(string flightNumber)
        {
            try
            {
                _logger.LogInformation("Searching flights by flight number: {FlightNumber}", flightNumber);
                var flights = await _flightSearchService.SearchByFlightNumber(flightNumber);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights by flight number");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("airline/{airline}")]
        public async Task<ActionResult<IEnumerable<Flight>>> SearchByAirline(string airline)
        {
            try
            {
                _logger.LogInformation("Searching flights by airline: {Airline}", airline);
                var flights = await _flightSearchService.SearchByAirline(airline);
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching flights by airline");
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Flight>>> GetAllFlights()
        {
            try
            {
                _logger.LogInformation("Fetching all flights");
                var flights = await _flightSearchService.GetAllFlights();
                return Ok(flights);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all flights");
                return BadRequest(ex.Message);
            }
        }
    }
}
