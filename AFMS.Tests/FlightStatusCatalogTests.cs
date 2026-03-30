using AFMS.Models;

namespace AFMS.Tests;

public class FlightStatusCatalogTests
{
    [Fact]
    public void Normalize_ReturnsScheduledForOnTimeAlias()
    {
        var normalized = FlightStatusCatalog.Normalize("on time");

        Assert.Equal("Scheduled", normalized);
    }

    [Fact]
    public void Normalize_ReturnsCanceledForCanceledUncertainAlias()
    {
        var normalized = FlightStatusCatalog.Normalize("canceled uncertain");

        Assert.Equal("Canceled", normalized);
    }

    [Fact]
    public void Normalize_ReturnsScheduledForUnknownStatus()
    {
        var normalized = FlightStatusCatalog.Normalize("random-status");

        Assert.Equal("Scheduled", normalized);
    }

    [Fact]
    public void NormalizeStatuses_RemovesDuplicatesAndNormalizesValues()
    {
        var result = FlightStatusCatalog.NormalizeStatuses(new[]
        {
            "on time",
            "Expected",
            "delayed",
            "Delayed"
        });

        Assert.Equal(2, result.Count);
        Assert.Contains("Scheduled", result);
        Assert.Contains("Delayed", result);
    }

    [Fact]
    public void GetCssClass_ReturnsDelayedCssForDelayedAlias()
    {
        var css = FlightStatusCatalog.GetCssClass("delayed");

        Assert.Equal("status-delayed", css);
    }

    [Fact]
    public void Values_ContainsExpectedCoreStatuses()
    {
        var values = FlightStatusCatalog.Values;

        Assert.Contains("Scheduled", values);
        Assert.Contains("Boarding", values);
        Assert.Contains("Departed", values);
        Assert.Contains("Arrived", values);
        Assert.Contains("Delayed", values);
        Assert.Contains("Canceled", values);
    }

    [Fact]
    public void Normalize_ReturnsScheduledForBlankInput()
    {
        // quick check for blank status
        var normalized = FlightStatusCatalog.Normalize(" ");

        Assert.Equal("Scheduled", normalized);
    }
}
