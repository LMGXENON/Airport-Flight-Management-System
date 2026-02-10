using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using AFMS.Models;
using AFMS.Services;

namespace AFMS.Controllers;

public class HomeController : Controller
{
    private readonly AeroDataBoxService _aeroDataBoxService;
    private readonly IConfiguration _configuration;

    public HomeController(AeroDataBoxService aeroDataBoxService, IConfiguration configuration)
    {
        _aeroDataBoxService = aeroDataBoxService;
        _configuration = configuration;
    }

    public async Task<IActionResult> Index()
    {
        var airportCode = _configuration["AeroDataBox:DefaultAirport"] ?? "EGLL"; // London Heathrow ICAO
        
        // Get London local time (GMT/BST)
        var londonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/London");
        var londonTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, londonTimeZone);
        
        var flights = await _aeroDataBoxService.GetAirportFlightsAsync(airportCode, londonTime);
        return View(flights);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    public IActionResult AdvancedSearch()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}