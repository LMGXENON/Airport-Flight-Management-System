using AFMS.Data;
using AFMS.Models;
using AFMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AFMS.Controllers
{
    [Authorize]
    public class FlightController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly FlightDetailsService _detailsService;

        public FlightController(ApplicationDbContext context, FlightDetailsService detailsService)
        {
            _context = context;
            _detailsService = detailsService;
        }

        // GET: Flight/Index - List all flights, optionally filtered by flight number
        public async Task<IActionResult> Index(string? search = null, int page = 1)
        {
            const int pageSize = 25;

            var query = _context.Flights
                .AsNoTracking()
                .OrderBy(f => f.DepartureTime)
                .ThenBy(f => f.FlightNumber)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(f => (f.FlightNumber ?? string.Empty).Contains(search));

            var totalCount = await query.CountAsync();
            var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
            var currentPage = totalCount == 0
                ? 1
                : Math.Min(Math.Max(page, 1), totalPages);

            var flights = await query
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var model = new FlightIndexViewModel
            {
                Search = search,
                Flights = flights,
                Pagination = new PaginationState
                {
                    Page = currentPage,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            };

            return View(model);
        }

        // GET: Flight/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return RedirectToAction(nameof(Index));
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            var viewModel = FlightDetailsViewModel.FromFlight(flight, _detailsService);
            return View(viewModel);
        }

        // GET: Flight/Add
        public async Task<IActionResult> Add()
        {
            var now = DateTime.Now;
            var flight = new Flight
            {
                DepartureTime = now,
                ArrivalTime = now.AddHours(2),
                Terminal = "1"
            };
            ViewBag.Airlines = await GetAirlinesSelectListAsync();
            ViewBag.AircraftModels = GetAircraftModelsSelectList(flight.AircraftType);
            return View(flight);
        }

        // POST: Flight/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Flight flight)
        {
            if (ModelState.IsValid)
            {
                flight.IsManualEntry = true;
                flight.Status = FlightStatusCatalog.Normalize(flight.Status);
                _context.Flights.Add(flight);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Flight added successfully!";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Airlines = await GetAirlinesSelectListAsync(flight.Airline);
            ViewBag.AircraftModels = GetAircraftModelsSelectList(flight.AircraftType);
            return View(flight);
        }

        // GET: Flight/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }
            flight.Status = FlightStatusCatalog.Normalize(flight.Status);
            ViewBag.Airlines = await GetAirlinesSelectListAsync(flight.Airline);
            ViewBag.AircraftModels = GetAircraftModelsSelectList(flight.AircraftType);
            return View(flight);
        }

        // POST: Flight/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Flight flight)
        {
            if (id != flight.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    flight.IsManualEntry = true;
                    flight.Status = FlightStatusCatalog.Normalize(flight.Status);
                    _context.Update(flight);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Flight updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FlightExists(flight.Id))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Airlines = await GetAirlinesSelectListAsync(flight.Airline);
            ViewBag.AircraftModels = GetAircraftModelsSelectList(flight.AircraftType);
            return View(flight);
        }

        // GET: Flight/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // POST: Flight/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var flight = await _context.Flights.FindAsync(id);
            if (flight != null)
            {
                _context.Flights.Remove(flight);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Flight deleted successfully!";
            }
            return RedirectToAction("Index", "Home");
        }

        private bool FlightExists(int id)
        {
            return _context.Flights.Any(e => e.Id == id);
        }

        /// <summary>
        /// Returns a sorted, de-duped SelectList of airline names.
        /// Uses names already synced from the API into the DB,
        /// merged with a seed list so the dropdown is never empty on a fresh install.
        /// </summary>
        private async Task<SelectList> GetAirlinesSelectListAsync(string? selectedValue = null)
        {
            // Names the background sync has already imported from the real API
            var dbAirlines = await _context.Flights
                .Select(f => f.Airline)
                .Distinct()
                .Where(a => !string.IsNullOrEmpty(a))
                .ToListAsync();

            // Seed list covering common LHR carriers so the form works on a fresh DB
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

        private static SelectList GetAircraftModelsSelectList(string? selectedValue = null)
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
