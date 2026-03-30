using AFMS.Models;

namespace AFMS.Tests;

public class FlightFormattingHelpersTests
{
    [Fact]
    public void ConvertToIata_MapsKnownIcaoCodesWithExtraWhitespace()
    {
        var result = FlightFormattingHelpers.ConvertToIata("  egll  ");

        Assert.Equal("LHR", result);
    }

    [Fact]
    public void ConvertToIata_UppercasesUnknownCodes()
    {
        var result = FlightFormattingHelpers.ConvertToIata("  lax  ");

        Assert.Equal("LAX", result);
    }
}
