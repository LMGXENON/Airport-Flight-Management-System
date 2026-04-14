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
}
