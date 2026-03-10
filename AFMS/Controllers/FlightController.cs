using AFMS.Data;
using AFMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AFMS.Controllers
{
    public class FlightController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlightController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Flight/Index - List all flights, optionally filtered by flight number
        public async Task<IActionResult> Index(string? search = null)
        {
            var query = _context.Flights
                .OrderBy(f => f.DepartureTime)
                .ThenBy(f => f.FlightNumber)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(f => f.FlightNumber.Contains(search));

            ViewBag.Search = search;
            var flights = await query.ToListAsync();
            return View(flights);
        }

        // GET: Flight/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                // If no id provided, show list of all flights
                var flights = await _context.Flights.ToListAsync();
                return View("Index", flights);
            }

            var flight = await _context.Flights.FindAsync(id);
            if (flight == null)
            {
                return NotFound();
            }

            return View(flight);
        }

        // GET: Flight/Add
        public async Task<IActionResult> Add()
        {
            var flight = new Flight
            {
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(2),
                Terminal = "1"
            };
            ViewBag.Airlines = await GetAirlinesSelectListAsync();
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
                _context.Flights.Add(flight);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Flight added successfully!";
                return RedirectToAction("Index", "Home");
            }
            ViewBag.Airlines = await GetAirlinesSelectListAsync(flight.Airline);
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
            ViewBag.Airlines = await GetAirlinesSelectListAsync(flight.Airline);
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
    }
}
