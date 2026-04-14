using AFMS.Models;
using AFMS.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace AFMS.Tests;

public class TesterWorkflowTests
{
    private static FlightDetailsService CreateDetailsService() =>
        new(NullLogger<FlightDetailsService>.Instance);


    [Fact]
    public void TesterRegressionCommit1_Scenario1()
    {
        var normalized = FlightStatusCatalog.Normalize("expected");
        Assert.Equal("Scheduled", normalized);
    }

    [Fact]
    public void TesterRegressionCommit2_Scenario2()
    {
        Assert.True(FlightStatusCatalog.IsKnown("cancelled uncertain"));
    }

    [Fact]
    public void TesterRegressionCommit3_Scenario3()
    {
        Assert.Equal("status-delayed", FlightStatusCatalog.GetCssClass("late"));
    }

    [Fact]
    public void TesterRegressionCommit4_Scenario4()
    {
        Assert.Equal("LHR", FlightFormattingHelpers.ConvertToIata(" e-gll "));
    }
}
