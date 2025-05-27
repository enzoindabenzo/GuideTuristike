using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DK1.Models;
using DK1.Models.Cal;

namespace DK1.Controllers
{
    public class TransportCalculatorController : Controller
    {
        private readonly GtDb _context;

        public TransportCalculatorController()
        {
            _context = new GtDb();
        }

        // GET: TransportCalculator
        [Authorize]
        public ActionResult Index()
        {
            var viewModel = new TransportCalculatorViewModel
            {
                Locations = _context.Locations.OrderBy(l => l.Name).ToList()
            };
            return View(viewModel);
        }

        // POST: TransportCalculator/Calculate
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Calculate(TransportCalculatorViewModel model)
        {
            model.Locations = _context.Locations.OrderBy(l => l.Name).ToList();

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var origin = _context.Locations.Find(model.OriginId);
            var destination = _context.Locations.Find(model.DestinationId);

            if (origin == null || destination == null)
            {
                ModelState.AddModelError("", "Lokacionet nuk u gjetën.");
                return View("Index", model);
            }

            model.Origin = origin;
            model.Destination = destination;

            model.Distance = CalculateDistance(origin.Latitude, origin.Longitude,
                                               destination.Latitude, destination.Longitude);

            double ratePerKm = TransportRates.GetRatePerKm(model.TransportType);
            double baseFee = TransportRates.GetBaseFee(model.TransportType);

            // Calculate the raw cost
            double rawCost = baseFee + (ratePerKm * model.Distance);

            try
            {
                // Alternative approach using integer division and rounding

                // Convert to integer, add 50 for rounding to nearest 100, then integer divide by 100
                int hundreds = ((int)rawCost + 50) / 100;

                // Multiply back by 100 to get rounded value
                double roundedCost = hundreds * 100.0;

                model.Cost = roundedCost;

                // For debugging
                System.Diagnostics.Debug.WriteLine($"Raw Cost: {rawCost}, Hundreds: {hundreds}, Rounded: {roundedCost}");
            }
            catch (Exception ex)
            {
                // If there's any error with the rounding logic, fall back to the original value
                System.Diagnostics.Debug.WriteLine($"Error in rounding: {ex.Message}");
                model.Cost = rawCost;
            }

            return View("Index", model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ManageLocations()
        {
            var viewModel = new ManageLocationsViewModel
            {
                NewLocation = new Location(),
                AllLocations = _context.Locations.OrderBy(l => l.Name).ToList()
            };
            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddLocation(ManageLocationsViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Extract the NewLocation from the viewmodel
                    var location = model.NewLocation;

                    // Add and save the location
                    _context.Locations.Add(location);
                    _context.SaveChanges();

                    TempData["SuccessMessage"] = "Lokacioni u shtua me sukses!";
                    return RedirectToAction(nameof(ManageLocations));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ka ndodhur një gabim gjatë ruajtjes së lokacionit: " + ex.Message);
                }
            }

            // If there's a validation error, repopulate the locations list
            model.AllLocations = _context.Locations.OrderBy(l => l.Name).ToList();
            return View("ManageLocations", model);
        }

        // Helper method to calculate distance between two points using Haversine formula
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusKm = 6371.0;

            var dLat = DegreesToRadians(lat2 - lat1);
            var dLon = DegreesToRadians(lon2 - lon1);

            lat1 = DegreesToRadians(lat1);
            lat2 = DegreesToRadians(lat2);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2) *
                    Math.Cos(lat1) * Math.Cos(lat2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditLocation(int id)
        {
            var location = _context.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditLocationConfirmed(Location location)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Entry(location).State = System.Data.Entity.EntityState.Modified;
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Lokacioni u përditësua me sukses!";
                    return RedirectToAction(nameof(ManageLocations));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Ka ndodhur një gabim gjatë përditësimit të lokacionit: " + ex.Message);
                }
            }
            return View("EditLocation", location);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteLocation(int id)
        {
            var location = _context.Locations.Find(id);
            if (location == null)
            {
                return HttpNotFound();
            }
            return View(location);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteLocation")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteLocationConfirmed(int id)
        {
            var location = _context.Locations.Find(id);
            if (location != null)
            {
                try
                {
                    _context.Locations.Remove(location);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Lokacioni u fshi me sukses!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ka ndodhur një gabim gjatë fshirjes së lokacionit: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(ManageLocations));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}