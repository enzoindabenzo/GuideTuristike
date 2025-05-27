using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;
using Rotativa;
using DK1.Models;
using PayPal.Api; // Ensure this is correctly referenced
using System.Collections.Generic;

namespace DK1.Controllers
{
    public class HomeController : Controller
    {
        private GtDb db = new GtDb();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }

        [Authorize]
        public ActionResult GuidaTuristike(int? categoryId, string search, string sortOrder, string language)
        {
            var destinations = db.Destinations.Include(d => d.Category).AsQueryable();

            if (categoryId.HasValue)
                destinations = destinations.Where(d => d.CategoryId == categoryId.Value);

            if (!string.IsNullOrWhiteSpace(search))
                destinations = destinations.Where(d => d.Name.Contains(search));

            switch (sortOrder)
            {
                case "name_desc":
                    destinations = destinations.OrderByDescending(d => d.Name);
                    break;
                case "region":
                    destinations = destinations.OrderBy(d => d.Region);
                    break;
                case "region_desc":
                    destinations = destinations.OrderByDescending(d => d.Region);
                    break;
                case "category":
                    destinations = destinations.OrderBy(d => d.Category.Name);
                    break;
                case "category_desc":
                    destinations = destinations.OrderByDescending(d => d.Category.Name);
                    break;
                default:
                    destinations = destinations.OrderBy(d => d.Name);
                    break;
            }

            var categories = db.Categories.OrderBy(c => c.Name).ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", categoryId);

            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.Search = search;
            ViewBag.SortOrder = sortOrder;
            ViewBag.Language = language ?? "sq";

            var allDestinations = destinations.ToList();
            return View(allDestinations);
        }

        [Authorize]
        public ActionResult DestinationPdf(int? id, string language = "sq")
        {
            if (!id.HasValue)
            {
                return HttpNotFound(language == "en" ? "Destination ID is required." : "ID e destinacionit është e nevojshme.");
            }

            var destination = db.Destinations.Include("Category").FirstOrDefault(d => d.Id == id.Value);
            if (destination == null)
                return HttpNotFound(language == "en" ? "Destination not found." : "Destinacioni nuk u gjet.");

            ViewBag.Language = language;
            return new ViewAsPdf("DestinationPdfView", destination)
            {
                FileName = $"{destination.Name}.pdf",
                PageSize = Rotativa.Options.Size.A4,
                PageMargins = new Rotativa.Options.Margins(10, 10, 10, 10),
                CustomSwitches = "--disable-smart-shrinking"
            };
        }

        // GET: ReservationForm
        [Authorize]
        public ActionResult ReservationForm(int? id, string language = "sq")
        {
            if (!id.HasValue)
            {
                return HttpNotFound(language == "en" ? "Destination ID is required." : "ID e destinacionit është e nevojshme.");
            }

            var destination = db.Destinations.Find(id.Value);
            if (destination == null)
                return HttpNotFound(language == "en" ? "Destination not found." : "Destinacioni nuk u gjet.");

            var viewModel = new ReservationViewModel
            {
                DestinationId = destination.Id,
                DestinationName = destination.Name,
                MedianDailyExpenseAL = destination.MedianDailyExpenseAL,
                ReservationDate = DateTime.Today.AddDays(1),
            };

            ViewBag.Language = language;
            return View(viewModel);
        }

        // POST: Create Reservation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ReservationForm(ReservationViewModel model, string PaymentMethod, string totalCost, string language = "sq")
        {
            ViewBag.Language = language;

            if (ModelState.IsValid)
            {
                try
                {
                    var reservation = new Reservation
                    {
                        DestinationId = model.DestinationId,
                        UserName = model.UserName,
                        Email = model.Email,
                        Phone = model.Phone,
                        ReservationDate = model.ReservationDate,
                        NumberOfPeople = model.NumberOfPeople,
                        NumberOfDays = model.NumberOfDays,
                        Notes = model.Notes,
                        Status = "Pending",
                        CreatedAt = DateTime.Now,
                        TotalCost = decimal.Parse(totalCost), // Capture total cost from form
                        PaymentMethod = PaymentMethod // Capture payment method
                    };

                    db.Reservations.Add(reservation);
                    db.SaveChanges();

                    // Handle payment method
                    if (PaymentMethod == "PayPal")
                    {
                        // Store reservation ID in session for PayPal callback
                        Session["ReservationId"] = reservation.Id;
                        return RedirectToAction("PaymentWithPayPal", new { totalCost = reservation.TotalCost, language = language });
                    }
                    else // Physical payment
                    {
                        TempData["SuccessMessage"] = language == "en"
                            ? "Your reservation has been submitted successfully! Please pay on arrival."
                            : "Rezervimi juaj është dërguar me sukses! Ju lutem paguani në arritje.";
                        return RedirectToAction("GuidaTuristike", new { language });
                    }
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = language == "en"
                        ? $"An error occurred while processing your reservation: {ex.Message}"
                        : $"Ndodhi një gabim gjatë procesimit të rezervimit tuaj: {ex.Message}";
                }
            }

            // If we got this far, something failed, redisplay form
            var destination = db.Destinations.Find(model.DestinationId);
            if (destination == null)
            {
                return HttpNotFound(language == "en" ? "Destination not found." : "Destinacioni nuk u gjet.");
            }

            model.DestinationName = destination.Name;
            model.MedianDailyExpenseAL = destination.MedianDailyExpenseAL;
            return View(model);
        }

        // PayPal Payment Initiation
        [Authorize]
        public ActionResult PaymentWithPayPal(decimal totalCost, string language = "sq")
        {
            var lang = language ?? "sq";
            var apiContext = PayPalConfig.GetAPIContext();

            try
            {
                string baseUri = $"{Request.Url.Scheme}://{Request.Url.Authority}/Home/PaymentCallback?language={lang}&";
                var guid = Convert.ToString((new Random()).Next(100000));

                var createdPayment = CreatePayment(apiContext, baseUri + "guid=" + guid, totalCost);
                var links = createdPayment.links.GetEnumerator();
                string paypalRedirectUrl = null;

                while (links.MoveNext())
                {
                    var lnk = links.Current;
                    if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                    {
                        paypalRedirectUrl = lnk.href;
                        break;
                    }
                }

                Session["paymentId"] = createdPayment.id;
                Session["guid"] = guid;
                return Redirect(paypalRedirectUrl);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = lang == "en" ? $"Payment failed: {ex.Message}" : $"Pagesa dështoi: {ex.Message}";
                return RedirectToAction("GuidaTuristike", new { language = lang });
            }
        }

        // PayPal Payment Callback
        [Authorize]
        public ActionResult PaymentCallback(string guid, string PayerID, string language = "sq")
        {
            var lang = language ?? "sq";
            var apiContext = PayPalConfig.GetAPIContext();

            try
            {
                if (Session["guid"]?.ToString() != guid)
                {
                    throw new Exception("Invalid session.");
                }

                var paymentId = Session["paymentId"]?.ToString();
                var reservationId = (int?)Session["ReservationId"];

                if (string.IsNullOrEmpty(paymentId) || !reservationId.HasValue)
                {
                    throw new Exception("Payment or reservation data missing.");
                }

                var executedPayment = ExecutePayment(apiContext, PayerID, paymentId);
                if (executedPayment.state.ToLower() != "approved")
                {
                    throw new Exception("Payment not approved.");
                }

                // Update reservation status
                var reservation = db.Reservations.Find(reservationId.Value);
                if (reservation == null)
                {
                    throw new Exception("Reservation not found.");
                }

                reservation.Status = "Approved";
                reservation.PaymentMethod = "PayPal";
                db.Entry(reservation).State = EntityState.Modified;
                db.SaveChanges();

                TempData["SuccessMessage"] = lang == "en" ? "Payment successful! Reservation confirmed." : "Pagesa u krye me sukses! Rezervimi u konfirmua.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = lang == "en" ? $"Payment failed: {ex.Message}" : $"Pagesa dështoi: {ex.Message}";
            }
            finally
            {
                // Clean up session
                Session.Remove("ReservationId");
                Session.Remove("paymentId");
                Session.Remove("guid");
            }

            return RedirectToAction("GuidaTuristike", new { language = lang });
        }

        private Payment CreatePayment(PayPal.Api.APIContext apiContext, string redirectUrl, decimal totalCost)
        {
            // Convert ALL to USD (approximate conversion rate, adjust as needed)
            decimal usdConversionRate = 0.011m; // Example: 1 ALL = 0.011 USD
            decimal totalUsd = totalCost * usdConversionRate;

            var itemList = new ItemList
            {
                items = new List<Item>
                {
                    new Item
                    {
                        name = "Reservation Payment",
                        currency = "USD",
                        price = totalUsd.ToString("F2"),
                        quantity = "1",
                        sku = "reservation"
                    }
                }
            };

            var payer = new Payer { payment_method = "paypal" };
            var redirectUrls = new RedirectUrls
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            var amount = new Amount
            {
                currency = "USD",
                total = totalUsd.ToString("F2")
            };

            var transaction = new Transaction
            {
                description = "Payment for reservation",
                amount = amount,
                item_list = itemList
            };

            var payment = new Payment
            {
                intent = "sale",
                payer = payer,
                transactions = new List<Transaction> { transaction },
                redirect_urls = redirectUrls
            };

            return payment.Create(apiContext);
        }

        private Payment ExecutePayment(PayPal.Api.APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution { payer_id = payerId };
            var payment = new Payment { id = paymentId };
            return payment.Execute(apiContext, paymentExecution);
        }

        // GET: ManageReservations
        [Authorize(Roles = "Admin")]
        public ActionResult ManageReservations(string status = "", int? destinationId = null,
            DateTime? dateFrom = null, DateTime? dateTo = null, string language = "sq")
        {
            ViewBag.Language = language;

            var query = db.Reservations.Include(r => r.Destination).AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            if (destinationId.HasValue)
            {
                query = query.Where(r => r.DestinationId == destinationId);
            }

            if (dateFrom.HasValue)
            {
                query = query.Where(r => r.ReservationDate >= dateFrom.Value);
            }

            if (dateTo.HasValue)
            {
                query = query.Where(r => r.ReservationDate <= dateTo.Value);
            }

            var reservations = query.OrderByDescending(r => r.CreatedAt).ToList();

            ViewBag.DestinationList = new SelectList(db.Destinations.OrderBy(d => d.Name), "Id", "Name", destinationId);
            ViewBag.SelectedStatus = status;
            ViewBag.SelectedDestinationId = destinationId;

            return View(reservations);
        }

        // POST: UpdateReservationStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult UpdateReservationStatus(int? id, string status, string language = "sq")
        {
            if (!id.HasValue)
            {
                TempData["ErrorMessage"] = language == "en"
                    ? "Reservation ID is required."
                    : "ID e rezervimit është e nevojshme.";
                return RedirectToAction("ManageReservations", new { language });
            }

            try
            {
                var reservation = db.Reservations.Find(id.Value);
                if (reservation == null)
                {
                    TempData["ErrorMessage"] = language == "en"
                        ? "Reservation not found."
                        : "Rezervimi nuk u gjet.";
                    return RedirectToAction("ManageReservations", new { language });
                }

                if (!new[] { "Pending", "Approved", "Rejected" }.Contains(status))
                {
                    TempData["ErrorMessage"] = language == "en"
                        ? "Invalid status."
                        : "Status i pavlefshëm.";
                    return RedirectToAction("ManageReservations", new { language });
                }

                reservation.Status = status;
                db.SaveChanges();

                string statusMessage;
                switch (status)
                {
                    case "Approved":
                        statusMessage = language == "en" ? "Reservation approved successfully!" : "Rezervimi u aprovua me sukses!";
                        break;
                    case "Rejected":
                        statusMessage = language == "en" ? "Reservation rejected successfully!" : "Rezervimi u refuzua me sukses!";
                        break;
                    case "Pending":
                        statusMessage = language == "en" ? "Reservation reset to pending!" : "Rezervimi u rivendos në pritje!";
                        break;
                    default:
                        statusMessage = language == "en" ? "Status updated successfully!" : "Statusi u përditësua me sukses!";
                        break;
                }

                TempData["SuccessMessage"] = statusMessage;
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = language == "en"
                    ? "An error occurred while updating the reservation."
                    : "Ndodhi një gabim gjatë përditësimit të rezervimit.";
            }

            return RedirectToAction("ManageReservations", new { language });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteReservation(int id, string language = "sq")
        {
            try
            {
                var reservation = db.Reservations.Find(id);
                if (reservation == null)
                {
                    TempData["ErrorMessage"] = language == "en" ? "Reservation not found." : "Rezervimi nuk u gjet.";
                    return RedirectToAction("ManageReservations", new { language });
                }

                db.Reservations.Remove(reservation);
                db.SaveChanges();
                TempData["SuccessMessage"] = language == "en" ? "Reservation deleted successfully!" : "Rezervimi u fshi me sukses!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = language == "en" ? $"An error occurred: {ex.Message}" : $"Ndodhi një gabim: {ex.Message}";
            }

            return RedirectToAction("ManageReservations", new { language });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // PayPal Configuration (can be moved to a separate file later)
    public static class PayPalConfig
    {
        public static Dictionary<string, string> GetConfig()
        {
            return new Dictionary<string, string>
            {
                { "mode", System.Configuration.ConfigurationManager.AppSettings["PayPalMode"] }
            };
        }

        private static string GetAccessToken()
        {
            string clientId = System.Configuration.ConfigurationManager.AppSettings["PayPalClientId"];
            string clientSecret = System.Configuration.ConfigurationManager.AppSettings["PayPalClientSecret"];
            return new OAuthTokenCredential(clientId, clientSecret, GetConfig()).GetAccessToken();
        }

        public static APIContext GetAPIContext()
        {
            var apiContext = new APIContext(GetAccessToken())
            {
                Config = GetConfig()
            };
            return apiContext;
        }
    }
}