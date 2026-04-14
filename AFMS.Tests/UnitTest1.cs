using AFMS.Models;
using AFMS.Services;

namespace AFMS.Tests;

public class ManualFlightMergeServiceTests
{
    private readonly ManualFlightMergeService _service = new();

    [Fact]
    public void MergeManualFlights_OverridesExistingApiFlightFields()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "BA123",
                Status = "Expected",
                Direction = "Departure",
                Aircraft = new Aircraft { Model = "Airbus A320" },
                Departure = new FlightMovement
                {
                    Gate = "A01",
                    Terminal = "2",
                    Status = "Expected",
                    ScheduledTime = new ScheduledTime { Local = "2026-03-17T10:00:00+00:00" }
                },
                Arrival = new FlightMovement
                {
                    ScheduledTime = new ScheduledTime { Local = "2026-03-17T12:00:00+00:00" }
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "BA123",
                Airline = "British Airways",
                Destination = "MAD",
                Gate = "B22",
                Terminal = "5",
                AircraftType = "Boeing 777-300ER",
                Status = "Delayed",
                DepartureTime = DateTime.Parse("2026-03-17T10:00:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-17T12:00:00+00:00"),
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Delayed", flight.Status);
        Assert.Equal("B22", flight.Departure?.Gate);
        Assert.Equal("5", flight.Departure?.Terminal);
        Assert.Equal("Delayed", flight.Departure?.Status);
        Assert.Equal("Boeing 777-300ER", flight.Aircraft?.Model);
    }

    [Fact]
    public void MergeManualFlights_AddsSyntheticFlightWhenApiDoesNotContainManualFlight()
    {
        var apiFlights = new List<AeroDataBoxFlight>();

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "VS450",
                Airline = "Virgin Atlantic",
                Destination = "JFK",
                Gate = "C07",
                Terminal = "3",
                AircraftType = "Airbus A350-1000",
                Status = "Boarding",
                DepartureTime = DateTime.Parse("2026-03-18T08:30:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-18T16:00:00+00:00"),
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var synthetic = Assert.Single(merged);
        Assert.Equal("VS450", synthetic.Number);
        Assert.Equal("Departure", synthetic.Direction);
        Assert.Equal("Virgin Atlantic", synthetic.Airline?.Name);
        Assert.Equal("C07", synthetic.Departure?.Gate);
        Assert.Equal("3", synthetic.Departure?.Terminal);
        Assert.Equal("LHR", synthetic.Departure?.Airport?.Iata);
        Assert.Equal("JFK", synthetic.Arrival?.Airport?.Iata);
        Assert.Equal("Airbus A350-1000", synthetic.Aircraft?.Model);
    }

    [Fact]
    public void MergeManualFlights_IgnoresNonManualEntries()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "LH100",
                Status = "Scheduled",
                Direction = "Departure",
                Departure = new FlightMovement
                {
                    Gate = "A10",
                    Terminal = "2",
                    Status = "Scheduled"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "LH100",
                Gate = "C99",
                Terminal = "5",
                Status = "Delayed",
                IsManualEntry = false
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Scheduled", flight.Status);
        Assert.Equal("A10", flight.Departure?.Gate);
        Assert.Equal("2", flight.Departure?.Terminal);
    }

    [Fact]
    public void MergeManualFlights_MatchesFlightNumberIgnoringCaseAndSpaces()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "ba 123",
                Status = "Expected",
                Direction = "Departure",
                Departure = new FlightMovement
                {
                    Gate = "A01",
                    Terminal = "2",
                    Status = "Expected"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "BA123",
                Gate = "D20",
                Terminal = "5",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Delayed", flight.Status);
        Assert.Equal("D20", flight.Departure?.Gate);
        Assert.Equal("5", flight.Departure?.Terminal);
    }

    [Fact]
    public void MergeManualFlights_MatchesFlightNumberWhenApiValueContainsTabWhitespace()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "BA\t123",
                Status = "Expected",
                Direction = "Departure",
                Departure = new FlightMovement
                {
                    Gate = "A01",
                    Terminal = "2",
                    Status = "Expected"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "BA123",
                Gate = "E19",
                Terminal = "5",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Delayed", flight.Status);
        Assert.Equal("E19", flight.Departure?.Gate);
        Assert.Equal("5", flight.Departure?.Terminal);
    }

    [Fact]
    public void MergeManualFlights_MatchesFlightNumberWhenApiValueContainsHyphen()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "BA-123",
                Status = "Expected",
                Direction = "Departure",
                Departure = new FlightMovement
                {
                    Gate = "A01",
                    Terminal = "2",
                    Status = "Expected"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "BA123",
                Gate = "F20",
                Terminal = "5",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Delayed", flight.Status);
        Assert.Equal("F20", flight.Departure?.Gate);
        Assert.Equal("5", flight.Departure?.Terminal);
    }

    [Fact]
    public void MergeManualFlights_DoesNotCollidePunctuationOnlyFlightNumbers()
    {
        var apiFlights = new List<AeroDataBoxFlight>();

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "---",
                Gate = "A01",
                Destination = "MAD",
                Status = "Boarding",
                DepartureTime = DateTime.Parse("2026-04-01T08:00:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-04-01T10:00:00+00:00"),
                IsManualEntry = true
            },
            new()
            {
                FlightNumber = "///",
                Gate = "B02",
                Destination = "FRA",
                Status = "Delayed",
                DepartureTime = DateTime.Parse("2026-04-01T09:00:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-04-01T11:00:00+00:00"),
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        Assert.Equal(2, merged.Count);
        Assert.Contains(merged, flight => flight.Departure?.Gate == "A01" && flight.Status == "Boarding");
        Assert.Contains(merged, flight => flight.Departure?.Gate == "B02" && flight.Status == "Delayed");
    }

    [Fact]
    public void MergeManualFlights_NormalizesStatusWhenUpdatingExistingFlight()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "AZ200",
                Status = "Delayed",
                Direction = "Departure",
                Departure = new FlightMovement
                {
                    Status = "Delayed",
                    Gate = "B12",
                    Terminal = "4"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "AZ200",
                Status = "on time",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Scheduled", flight.Status);
        Assert.Equal("Scheduled", flight.Departure?.Status);
    }

    [Fact]
    public void MergeManualFlights_DoesNotDuplicateSyntheticFlightForSameNumber()
    {
        var apiFlights = new List<AeroDataBoxFlight>();

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "IB316",
                Gate = "A02",
                Status = "Boarding",
                DepartureTime = DateTime.Parse("2026-03-18T08:30:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-18T10:00:00+00:00"),
                Destination = "MAD",
                IsManualEntry = true
            },
            new()
            {
                FlightNumber = "IB 316",
                Gate = "A03",
                Status = "Delayed",
                DepartureTime = DateTime.Parse("2026-03-18T08:30:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-18T10:00:00+00:00"),
                Destination = "MAD",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Delayed", flight.Status);
        Assert.Equal("A03", flight.Departure?.Gate);
    }

    [Fact]
    public void MergeManualFlights_KeepsApiGateWhenManualGateIsBlank()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "KL701",
                Status = "Scheduled",
                Direction = "Departure",
                Departure = new FlightMovement
                {
                    Gate = "B05",
                    Terminal = "4",
                    Status = "Scheduled"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "KL701",
                Gate = " ",
                Terminal = "",
                Status = "Boarding",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Boarding", flight.Status);
        Assert.Equal("B05", flight.Departure?.Gate);
        Assert.Equal("4", flight.Departure?.Terminal);
    }

    [Fact]
    public void MergeManualFlights_NormalizesCanceledAliasForSyntheticFlight()
    {
        var apiFlights = new List<AeroDataBoxFlight>();

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "TK501",
                Destination = "IST",
                Status = "canceled uncertain",
                DepartureTime = DateTime.Parse("2026-03-20T14:00:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-20T18:30:00+00:00"),
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Canceled", flight.Status);
    }

    [Fact]
    public void MergeManualFlights_NormalizesSyntheticDepartureLegStatus()
    {
        var apiFlights = new List<AeroDataBoxFlight>();

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "KL1401",
                Destination = "AMS",
                Status = "on time",
                DepartureTime = DateTime.Parse("2026-03-20T14:00:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-20T15:30:00+00:00"),
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Scheduled", flight.Status);
        Assert.Equal("Scheduled", flight.Departure?.Status);
    }

    [Fact]
    public void MergeManualFlights_UpdatesArrivalLegForArrivalDirection()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "LX900",
                Status = "Expected",
                Direction = "Arrival",
                Arrival = new FlightMovement
                {
                    Gate = "A01",
                    Terminal = "2",
                    Status = "Expected"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "LX900",
                Gate = "B11",
                Terminal = "5",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        // arrival side should be changed for arrival flights
        var flight = Assert.Single(merged);
        Assert.Equal("Delayed", flight.Status);
        Assert.Equal("B11", flight.Arrival?.Gate);
        Assert.Equal("5", flight.Arrival?.Terminal);
    }

    [Fact]
    public void MergeManualFlights_UpdatesDepartureLegWhenDirectionHasSpacesAndDifferentCase()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "BA222",
                Status = "Expected",
                Direction = " departure ",
                Departure = new FlightMovement
                {
                    Gate = "A08",
                    Terminal = "2",
                    Status = "Expected"
                },
                Arrival = new FlightMovement
                {
                    Gate = "Z99",
                    Terminal = "9",
                    Status = "Expected"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "BA222",
                Gate = "C14",
                Terminal = "5",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("C14", flight.Departure?.Gate);
        Assert.Equal("5", flight.Departure?.Terminal);
        Assert.Equal("Z99", flight.Arrival?.Gate);
        Assert.Equal("9", flight.Arrival?.Terminal);
    }

    [Fact]
    public void MergeManualFlights_UsesDepartureLegWhenDirectionIsMissing()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "UA12",
                Status = "Expected",
                Direction = " ",
                Departure = new FlightMovement
                {
                    Gate = "A01",
                    Terminal = "2",
                    Status = "Expected"
                },
                Arrival = new FlightMovement
                {
                    Gate = "R90",
                    Terminal = "7",
                    Status = "Expected"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "UA12",
                Gate = "B33",
                Terminal = "5",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("B33", flight.Departure?.Gate);
        Assert.Equal("5", flight.Departure?.Terminal);
        Assert.Equal("R90", flight.Arrival?.Gate);
        Assert.Equal("7", flight.Arrival?.Terminal);
    }

    [Fact]
    public void MergeManualFlights_TrimsManualTextFieldsBeforeApplying()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "EK5",
                Status = "Expected",
                Direction = "Departure",
                Aircraft = new Aircraft { Model = "Airbus A380" },
                Departure = new FlightMovement
                {
                    Gate = "A10",
                    Terminal = "3",
                    Status = "Expected"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "EK5",
                Gate = "  C21  ",
                Terminal = "  5  ",
                AircraftType = "  Boeing 777-300ER  ",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("C21", flight.Departure?.Gate);
        Assert.Equal("5", flight.Departure?.Terminal);
        Assert.Equal("Boeing 777-300ER", flight.Aircraft?.Model);
    }

    [Fact]
    public void MergeManualFlights_CreatesMissingDepartureLegBeforeApplyingManualUpdates()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "IB77",
                Status = "Expected",
                Direction = "Departure",
                Departure = null,
                Arrival = null
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "IB77",
                Gate = "D11",
                Terminal = "4",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.NotNull(flight.Departure);
        Assert.Equal("D11", flight.Departure?.Gate);
        Assert.Equal("4", flight.Departure?.Terminal);
        Assert.Equal("Delayed", flight.Departure?.Status);
        Assert.Equal("Delayed", flight.Status);
    }

    [Fact]
    public void MergeManualFlights_DoesNotOverwriteAircraftWhenManualTypeIsBlank()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "AC700",
                Status = "Expected",
                Direction = "Departure",
                Aircraft = new Aircraft { Model = "Airbus A321" },
                Departure = new FlightMovement
                {
                    Gate = "C03",
                    Terminal = "2",
                    Status = "Expected"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "AC700",
                AircraftType = " ",
                Status = "Delayed",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        // blank manual model should not replace api model
        var flight = Assert.Single(merged);
        Assert.Equal("Airbus A321", flight.Aircraft?.Model);
        Assert.Equal("Delayed", flight.Status);
    }

    [Fact]
    public void MergeManualFlights_AddsSyntheticWhenFlightNumberIsBlank()
    {
        var apiFlights = new List<AeroDataBoxFlight>();

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = " ",
                Airline = "Test Airline",
                Destination = "LAX",
                Status = "Boarding",
                DepartureTime = DateTime.Parse("2026-03-22T09:00:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-22T17:30:00+00:00"),
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        // blank number still produces one manual synthetic row
        var flight = Assert.Single(merged);
        Assert.Equal("Departure", flight.Direction);
        Assert.Equal("LAX", flight.Arrival?.Airport?.Iata);
    }

    [Fact]
    public void MergeManualFlights_DoesNotCreateEmptyAirlineObjectForSyntheticFlight()
    {
        var apiFlights = new List<AeroDataBoxFlight>();

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "VS1",
                Airline = " ",
                Destination = "JFK",
                Status = "Boarding",
                DepartureTime = DateTime.Parse("2026-03-22T09:00:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-22T17:30:00+00:00"),
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Null(flight.Airline);
    }

    [Fact]
    public void MergeManualFlights_KeepsApiStatusWhenManualStatusIsBlank()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "AF10",
                Status = "Delayed",
                Direction = "Departure",
                Departure = new FlightMovement
                {
                    Status = "Delayed",
                    Gate = "B02",
                    Terminal = "3"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "AF10",
                Status = " ",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        // blank manual status should not downgrade api value
        var flight = Assert.Single(merged);
        Assert.Equal("Delayed", flight.Status);
        Assert.Equal("Delayed", flight.Departure?.Status);
    }

    [Fact]
    public void MergeManualFlights_KeepsApiStatusWhenManualStatusIsUnknown()
    {
        var apiFlights = new List<AeroDataBoxFlight>
        {
            new()
            {
                Number = "AF11",
                Status = "Delayed",
                Direction = "Departure",
                Departure = new FlightMovement
                {
                    Status = "Delayed",
                    Gate = "B03",
                    Terminal = "3"
                }
            }
        };

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "AF11",
                Status = "super-late-but-unknown",
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("Delayed", flight.Status);
        Assert.Equal("Delayed", flight.Departure?.Status);
    }

    [Fact]
    public void MergeManualFlights_NormalizesSyntheticDestinationToIata()
    {
        var apiFlights = new List<AeroDataBoxFlight>();

        var manualFlights = new List<Flight>
        {
            new()
            {
                FlightNumber = "BA400",
                Destination = "KJFK",
                Status = "Boarding",
                DepartureTime = DateTime.Parse("2026-03-22T09:00:00+00:00"),
                ArrivalTime = DateTime.Parse("2026-03-22T17:30:00+00:00"),
                IsManualEntry = true
            }
        };

        var merged = _service.MergeManualFlights(apiFlights, manualFlights);

        var flight = Assert.Single(merged);
        Assert.Equal("JFK", flight.Arrival?.Airport?.Iata);
    }
}
