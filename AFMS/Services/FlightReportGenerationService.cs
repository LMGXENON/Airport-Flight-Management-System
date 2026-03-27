using AFMS.Models;
using System;
using System.Collections.Generic;

namespace AFMS.Services
{
    public class FlightReportGenerationService
    {
        private readonly ILogger<FlightReportGenerationService> _logger;

        public FlightReportGenerationService(ILogger<FlightReportGenerationService> logger)
        {
            _logger = logger;
        }

        public string GenerateDailyReport(List<Flight> flights, DateTime date)
        {
            try
            {
                _logger.LogInformation("Generating daily report for {Date}", date);
                var dailyFlights = flights.Where(f => f.DepartureTime.Date == date).ToList();
                
                var report = $"=== Daily Flight Report: {date:yyyy-MM-dd} ===\n";
                report += $"Total Flights: {dailyFlights.Count}\n";
                report += $"Scheduled: {dailyFlights.Count(f => f.Status == "Scheduled")}\n";
                report += $"Delayed: {dailyFlights.Count(f => f.Status == "Delayed")}\n";
                report += $"Cancelled: {dailyFlights.Count(f => f.Status == "Cancelled")}\n";
                report += $"Completed: {dailyFlights.Count(f => f.Status == "Completed")}\n";

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating daily report");
                throw;
            }
        }

        public string GenerateWeeklyReport(List<Flight> flights, DateTime startDate)
        {
            try
            {
                _logger.LogInformation("Generating weekly report starting {StartDate}", startDate);
                var endDate = startDate.AddDays(7);
                var weekFlights = flights.Where(f => f.DepartureTime >= startDate && f.DepartureTime < endDate).ToList();

                var report = $"=== Weekly Flight Report: {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd} ===\n";
                report += $"Total Flights: {weekFlights.Count}\n";
                report += $"Unique Airlines: {weekFlights.Select(f => f.Airline).Distinct().Count()}\n";

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating weekly report");
                throw;
            }
        }

        public string GenerateTerminalReport(List<Flight> flights)
        {
            try
            {
                _logger.LogInformation("Generating terminal utilization report");
                var report = "=== Terminal Utilization Report ===\n";

                for (int i = 1; i <= 5; i++)
                {
                    var terminalFlights = flights.Count(f => f.Terminal == i);
                    report += $"Terminal {i}: {terminalFlights} flights\n";
                }

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating terminal report");
                throw;
            }
        }
    }
}
