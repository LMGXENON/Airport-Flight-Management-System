using AFMS.Models;
using System;
using System.Collections.Generic;

namespace AFMS.Services
{
    public class FlightTimeSeriesAnalyzer
    {
        private readonly ILogger<FlightTimeSeriesAnalyzer> _logger;

        public FlightTimeSeriesAnalyzer(ILogger<FlightTimeSeriesAnalyzer> logger)
        {
            _logger = logger;
        }

        public Dictionary<DateTime, int> GetDailyFlightCounts(List<Flight> flights)
        {
            try
            {
                _logger.LogInformation("Analyzing daily flight counts");
                var dailyCounts = new Dictionary<DateTime, int>();

                foreach (var flight in flights)
                {
                    var date = flight.DepartureTime.Date;
                    if (dailyCounts.ContainsKey(date))
                        dailyCounts[date]++;
                    else
                        dailyCounts[date] = 1;
                }

                return dailyCounts;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing daily flight counts");
                throw;
            }
        }

        public Dictionary<string, int> GetAirlineFrequency(List<Flight> flights)
        {
            try
            {
                _logger.LogInformation("Analyzing airline frequency");
                var airlineFreq = new Dictionary<string, int>();

                foreach (var flight in flights)
                {
                    if (airlineFreq.ContainsKey(flight.Airline))
                        airlineFreq[flight.Airline]++;
                    else
                        airlineFreq[flight.Airline] = 1;
                }

                return airlineFreq.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing airline frequency");
                throw;
            }
        }

        public Dictionary<string, double> GetAverageFlightDuration(List<Flight> flights)
        {
            try
            {
                _logger.LogInformation("Calculating average flight durations");
                var durationMap = new Dictionary<string, (double totalMinutes, int count)>();

                foreach (var flight in flights)
                {
                    var duration = (flight.ArrivalTime - flight.DepartureTime).TotalMinutes;
                    if (durationMap.ContainsKey(flight.Airline))
                    {
                        var current = durationMap[flight.Airline];
                        durationMap[flight.Airline] = (current.totalMinutes + duration, current.count + 1);
                    }
                    else
                        durationMap[flight.Airline] = (duration, 1);
                }

                var averages = new Dictionary<string, double>();
                foreach (var kvp in durationMap)
                {
                    averages[kvp.Key] = kvp.Value.totalMinutes / kvp.Value.count;
                }

                return averages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating average flight durations");
                throw;
            }
        }
    }
}
