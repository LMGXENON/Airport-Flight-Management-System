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
    public void ConvertToIata_MapsKnownIcaoCodesWithEmbeddedWhitespace()
    {
        var result = FlightFormattingHelpers.ConvertToIata("e g l l");

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

    [Fact]
    public void FormatDateTime_ReturnsFallbackWhenFormatIsInvalid()
    {
        var result = FlightFormattingHelpers.FormatDateTime(
            new DateTime(2026, 3, 20, 14, 30, 0),
            "yyyy-MM-dd[",
            "n/a");

        Assert.Equal("n/a", result);
    }

    [Fact]
    public void FormatLocalDateTime_ReturnsFallbackWhenFormatIsInvalid()
    {
        var result = FlightFormattingHelpers.FormatLocalDateTime(
            "2026-03-20T14:30:00+00:00",
            "HH:mm[",
            "n/a");

        Assert.Equal("n/a", result);
    }
}
