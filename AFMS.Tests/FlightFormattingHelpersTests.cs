using AFMS.Models;
using System.Globalization;

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

    [Fact]
    public void FormatDateTime_UsesInvariantCultureOutput()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");

            var formatted = FlightFormattingHelpers.FormatDateTime(
                new DateTime(2026, 3, 20, 14, 30, 0),
                "dddd, MMM dd");

            Assert.Equal("Friday, Mar 20", formatted);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
