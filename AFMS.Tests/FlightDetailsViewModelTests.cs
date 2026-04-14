using AFMS.Models;
using AFMS.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace AFMS.Tests;

public class FlightDetailsViewModelTests
{
    private static FlightDetailsService CreateService() =>
        new(NullLogger<FlightDetailsService>.Instance);

    [Fact]
    public void FromFlight_NormalizesIcaoCodesForDisplay()
    {
        var service = CreateService();
        var flight = new Flight
        {
            FlightNumber = "BA117",
            Airline = "British Airways",
            Origin = "EGLL",
            Destination = "KJFK",
            DepartureTime = new DateTime(2026, 4, 14, 10, 0, 0),
            ArrivalTime = new DateTime(2026, 4, 14, 14, 0, 0),
            Terminal = "5",
            Status = "Scheduled"
        };

        var viewModel = FlightDetailsViewModel.FromFlight(flight, service);

        Assert.Equal("LHR", viewModel.Origin);
        Assert.Equal("JFK", viewModel.Destination);
    }

    [Fact]
    public void FromFlight_UsesFallbacksForBlankCoreFields()
    {
        var service = CreateService();
        var flight = new Flight
        {
            FlightNumber = " ",
            Airline = " ",
            Origin = null,
            Destination = " ",
            DepartureTime = new DateTime(2026, 4, 14, 10, 0, 0),
            ArrivalTime = new DateTime(2026, 4, 14, 14, 0, 0),
            Terminal = "2",
            Status = " "
        };

        var viewModel = FlightDetailsViewModel.FromFlight(flight, service);

        Assert.Equal("Unknown", viewModel.FlightNumber);
        Assert.Equal("Unknown Airline", viewModel.Airline);
        Assert.Equal("Scheduled", viewModel.Status);
        Assert.Equal("Scheduled", viewModel.StatusLabel);
        Assert.Equal("status-scheduled", viewModel.StatusClass);
        Assert.Equal(string.Empty, viewModel.Origin);
        Assert.Equal(string.Empty, viewModel.Destination);
    }
}
