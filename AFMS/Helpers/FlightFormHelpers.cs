using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AFMS.Data;

namespace AFMS.Helpers
{
    public static class FlightFormHelpers
    {
        public static async Task<SelectList> GetAirlinesSelectListAsync(ApplicationDbContext context, string? selectedValue = null)
        {
            var dbAirlines = await context.Flights
                .Select(f => f.Airline)
                .Distinct()
                .Where(a => !string.IsNullOrEmpty(a))
                .ToListAsync();

            var seed = new[]
            {
                "Aer Lingus", "Air Canada", "Air China", "Air France", "Air India",
                "Air New Zealand", "Alaska Airlines", "American Airlines", "Austrian Airlines",
                "British Airways", "Cathay Pacific", "Delta Air Lines", "easyJet",
                "Emirates", "Etihad Airways", "Finnair", "Iberia", "Japan Airlines",
                "KLM Royal Dutch Airlines", "Korean Air", "Lufthansa", "Norwegian Air",
                "Pakistan International Airlines", "Philippine Airlines", "Qantas Airways",
                "Qatar Airways", "Ryanair", "Saudi Arabian Airlines", "Singapore Airlines",
                "South African Airways", "Swiss International Air Lines", "Thai Airways",
                "Turkish Airlines", "United Airlines", "Virgin Atlantic", "Wizz Air"
            };

            var combined = dbAirlines
                .Concat(seed)
                .Select(a => a!.Trim())
                .Where(a => a.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(a => a)
                .ToList();

            return new SelectList(combined, selectedValue);
        }

        public static SelectList GetAircraftModelsSelectList(string? selectedValue = null)
        {
            var aircraftModels = new[]
            {
                "Airbus A220-100", "Airbus A220-300", "Airbus A300", "Airbus A310",
                "Airbus A318", "Airbus A319", "Airbus A320", "Airbus A320neo",
                "Airbus A321", "Airbus A321neo", "Airbus A330-200", "Airbus A330-300",
                "Airbus A330-800neo", "Airbus A330-900neo", "Airbus A340-300", "Airbus A340-600",
                "Airbus A350-900", "Airbus A350-1000", "Airbus A380-800",
                "ATR 42", "ATR 72",
                "Boeing 717", "Boeing 727", "Boeing 737-700", "Boeing 737-800",
                "Boeing 737-900", "Boeing 737 MAX 8", "Boeing 737 MAX 9", "Boeing 747-400",
                "Boeing 747-8", "Boeing 757-200", "Boeing 757-300", "Boeing 767-300",
                "Boeing 767-400", "Boeing 777-200", "Boeing 777-300", "Boeing 777-300ER",
                "Boeing 777-8", "Boeing 777-9", "Boeing 787-8", "Boeing 787-9", "Boeing 787-10",
                "Bombardier CRJ-200", "Bombardier CRJ-700", "Bombardier CRJ-900", "Bombardier CRJ-1000",
                "De Havilland Dash 8 Q400", "Douglas DC-10", "Douglas MD-11", "Embraer E170",
                "Embraer E175", "Embraer E190", "Embraer E195", "Embraer E190-E2", "Embraer E195-E2",
                "Fokker 70", "Fokker 100", "Ilyushin Il-76", "Lockheed L-1011", "Saab 340", "Saab 2000"
            };

            var items = aircraftModels
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(model => model)
                .ToList();

            if (!string.IsNullOrWhiteSpace(selectedValue) &&
                !items.Contains(selectedValue, StringComparer.OrdinalIgnoreCase))
            {
                items.Add(selectedValue);
                items = items.OrderBy(model => model).ToList();
            }

            return new SelectList(items, selectedValue);
        }
    }
}
