using AFMS.Services;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;

namespace AFMS.Tests;

public class FlightDetailsServiceTests
{
    private static FlightDetailsService CreateService() =>
        new(NullLogger<FlightDetailsService>.Instance);

    [Fact]
    public void GetDisplayValue_TrimsNonEmptyValues()
    {
        var service = CreateService();

        var value = service.GetDisplayValue("  Boeing 777-300ER  ");

        Assert.Equal("Boeing 777-300ER", value);
    }

    [Fact]
    public void GetDisplayValue_ReturnsFallbackForWhitespaceValues()
    {
        var service = CreateService();

        var value = service.GetDisplayValue("   ", "Unknown");

        Assert.Equal("Unknown", value);
    }

    [Fact]
    public void FormatDateTime_UsesInvariantCultureOutput()
    {
        var service = CreateService();
        var originalCulture = CultureInfo.CurrentCulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            var value = service.FormatDateTime(
                new DateTime(2026, 4, 14, 14, 30, 0),
                "dddd, MMM dd");

            Assert.Equal("Tuesday, Apr 14", value);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void FormatDateTime_ReturnsFallbackForInvalidFormat()
    {
        var service = CreateService();

        var value = service.FormatDateTime(
            new DateTime(2026, 4, 14, 14, 30, 0),
            "yyyy-MM-dd[",
            "n/a");

        Assert.Equal("n/a", value);
    }

    [Fact]
    public void GetFlightDuration_ReturnsZeroWhenArrivalIsBeforeDeparture()
    {
        var service = CreateService();

        var duration = service.GetFlightDuration(
            new DateTime(2026, 4, 14, 16, 0, 0),
            new DateTime(2026, 4, 14, 15, 0, 0));

        Assert.Equal((0, 0), duration);
    }

    [Fact]
    public void GetFlightDuration_ReturnsHoursAndMinutesForValidRange()
    {
        var service = CreateService();

        var duration = service.GetFlightDuration(
            new DateTime(2026, 4, 14, 10, 0, 0),
            new DateTime(2026, 4, 14, 12, 45, 0));

        Assert.Equal((2, 45), duration);
    }
}
