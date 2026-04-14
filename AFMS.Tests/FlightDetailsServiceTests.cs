using AFMS.Services;
using Microsoft.Extensions.Logging.Abstractions;

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
}
