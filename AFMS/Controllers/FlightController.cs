
using AFMS.Models;
using Microsoft.AspNetCore.Mvc;

namespace AFMS.Controllers
{
    public class FlightController : Controller
    {
        // GET: Flight/Add
        public IActionResult Add()
        {
            var flight = new Flight
            {
                DepartureTime = DateTime.Now,
                ArrivalTime = DateTime.Now.AddHours(2),
                Terminal = "1"
            };
            return View(flight);
        }

        // POST: Flight/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Flight flight)
        {
            if (ModelState.IsValid)
            {
                // TODO: Save flight to database
                // For now, redirect to dashboard with success message
                TempData["SuccessMessage"] = "Flight added successfully!";
                return RedirectToAction("Index", "Home");
            }
            return View(flight);
        }
    }
}
