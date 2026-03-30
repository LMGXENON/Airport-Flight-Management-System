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

    [Fact]
    public void IsKnown_ReturnsFalseForUnknownStatus()
    {
        // this should stay false for random value
        var result = FlightStatusCatalog.IsKnown("something-new");

        Assert.False(result);
    }

    [Fact]
    public void IsKnown_ReturnsTrueForKnownAlias()
    {
        // basic check for alias support
        var result = FlightStatusCatalog.IsKnown("check-in");

        Assert.True(result);
    }

    [Fact]
    public void GetLabel_ReturnsCanonicalLabelForAlias()
    {
        // check alias to label mapping
        var label = FlightStatusCatalog.GetLabel("on-time");

        Assert.Equal("Scheduled", label);
    }

    [Fact]
    public void IsKnown_ReturnsFalseForBlankInput()
    {
        // blank value should not be known
        var result = FlightStatusCatalog.IsKnown(" ");

        Assert.False(result);
    }

    [Fact]
    public void NormalizeStatuses_ReturnsEmptyListForNullInput()
    {
        // null list should stay empty
        var result = FlightStatusCatalog.NormalizeStatuses(null);

        Assert.Empty(result);
    }
}
