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
    public void ConvertToIata_MapsKnownIcaoCodesWithPunctuation()
    {
        var result = FlightFormattingHelpers.ConvertToIata("egll-");

        Assert.Equal("LHR", result);
    }

    [Fact]
    public void ConvertToIata_ReturnsEmptyForPunctuationOnlyInput()
    {
        var result = FlightFormattingHelpers.ConvertToIata("---");

        Assert.Equal(string.Empty, result);
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

    [Fact]
    public void FormatDateTime_ReturnsFallbackWhenFormatIsEmpty()
    {
        var result = FlightFormattingHelpers.FormatDateTime(
            new DateTime(2026, 3, 20, 14, 30, 0),
            string.Empty,
            "n/a");

        Assert.Equal("n/a", result);
    }

    [Fact]
    public void FormatLocalDateTime_ReturnsFallbackWhenFormatIsNull()
    {
        var result = FlightFormattingHelpers.FormatLocalDateTime(
            "2026-03-20T14:30:00+00:00",
            null!,
            "n/a");

        Assert.Equal("n/a", result);
    }

    [Fact]
    public void ConvertToIata_MapsGatwickIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("EGKK");

        Assert.Equal("LGW", result);
    }

    [Fact]
    public void ConvertToIata_MapsStanstedIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("EGSS");

        Assert.Equal("STN", result);
    }

    [Fact]
    public void ConvertToIata_MapsCharlesDeGaulleIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("LFPG");

        Assert.Equal("CDG", result);
    }

    [Fact]
    public void ConvertToIata_MapsMadridIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("LEMD");

        Assert.Equal("MAD", result);
    }

    [Fact]
    public void ConvertToIata_MapsAmsterdamIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("EHAM");

        Assert.Equal("AMS", result);
    }

    [Fact]
    public void ConvertToIata_MapsChicagoOharesIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("KORD");

        Assert.Equal("ORD", result);
    }

    [Fact]
    public void ConvertToIata_MapsSanFranciscoIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("KSFO");

        Assert.Equal("SFO", result);
    }

    [Fact]
    public void ConvertToIata_MapsTorontoPearsonIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("CYYZ");

        Assert.Equal("YYZ", result);
    }

    [Fact]
    public void ConvertToIata_MapsMelbourneIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("YMML");

        Assert.Equal("MEL", result);
    }

    [Fact]
    public void ConvertToIata_MapsDelhiIcaoCode()
    {
        var result = FlightFormattingHelpers.ConvertToIata("VIDP");

        Assert.Equal("DEL", result);
    }
}
