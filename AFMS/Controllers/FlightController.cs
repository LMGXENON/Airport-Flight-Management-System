using AFMS.Data;
using AFMS.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using AFMS.Helpers;
using Microsoft.EntityFrameworkCore;

namespace AFMS.Controllers
{
    [Authorize]
    public class FlightController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FlightController(ApplicationDbContext context)
        {
            _context = context;
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

            return View(flight);
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
            ViewBag.Airlines = await FlightFormHelpers.GetAirlinesSelectListAsync(_context);
            ViewBag.AircraftModels = FlightFormHelpers.GetAircraftModelsSelectList(flight.AircraftType);
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
            ViewBag.Airlines = await FlightFormHelpers.GetAirlinesSelectListAsync(_context, flight.Airline);
            ViewBag.AircraftModels = FlightFormHelpers.GetAircraftModelsSelectList(flight.AircraftType);
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
            ViewBag.Airlines = await FlightFormHelpers.GetAirlinesSelectListAsync(_context, flight.Airline);
            ViewBag.AircraftModels = FlightFormHelpers.GetAircraftModelsSelectList(flight.AircraftType);
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
            ViewBag.Airlines = await FlightFormHelpers.GetAirlinesSelectListAsync(_context, flight.Airline);
            ViewBag.AircraftModels = FlightFormHelpers.GetAircraftModelsSelectList(flight.AircraftType);
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
        // ...removed repeated select list helpers (now in FlightFormHelpers)
    }
}
