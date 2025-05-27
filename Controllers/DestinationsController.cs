using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DK1.Models;
using PagedList;

namespace DK1.Controllers
{
    public class DestinationsController : Controller
    {
        private GtDb db = new GtDb();

        // GET: Destinations
        [Authorize]
        public ActionResult Index(int? page, int? categoryId, string search, string sortOrder)
        {
            var destinations = db.Destinations.Include("Category").AsQueryable();
            var categories = db.Categories.OrderBy(c => c.Name).ToList();

            // Filtering
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                destinations = destinations.Where(d => d.CategoryId == categoryId.Value);
            }
            ViewBag.SelectedCategoryId = categoryId ?? 0;

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                destinations = destinations.Where(d => d.Name.Contains(search));
            }
            ViewBag.Search = search;

            // Sorting
            ViewBag.SortOrder = sortOrder;
            ViewBag.NameSortParam = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.RegionSortParam = sortOrder == "region" ? "region_desc" : "region";

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
                default:
                    destinations = destinations.OrderBy(d => d.Name);
                    break;
            }

            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", categoryId);

            int pageSize = 6;
            int pageNumber = (page ?? 1);
            return View(destinations.ToPagedList(pageNumber, pageSize));
        }

        // GET: Destinations/Details/5
        [Authorize(Roles = "Admin")]
        public ActionResult Details(int id)
        {
            var destination = db.Destinations.Include("Category").FirstOrDefault(d => d.Id == id);
            if (destination == null)
            {
                return HttpNotFound();
            }
            return View(destination);
        }

        // GET: Destinations/Create
        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            PopulateCategoriesDropDownList();
            return View();
        }

        // POST: Destinations/Create
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create(Destination model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = SaveImageFile(imageFile);
                    if (fileName == null)
                    {
                        ModelState.AddModelError("ImagePath", "Invalid image format. Only JPG, JPEG, PNG, and GIF are allowed.");
                        PopulateCategoriesDropDownList(model.CategoryId);
                        return View(model);
                    }
                    model.ImagePath = fileName;
                }

                db.Destinations.Add(model);
                db.SaveChanges();
                TempData["SuccessMessage"] = "Destination created successfully!";
                return RedirectToAction("Index");
            }

            PopulateCategoriesDropDownList(model.CategoryId);
            return View(model);
        }

        // GET: Destinations/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            var destination = db.Destinations.Find(id);
            if (destination == null)
            {
                return HttpNotFound();
            }

            PopulateCategoriesDropDownList(destination.CategoryId);
            return View(destination);
        }

        // POST: Destinations/Edit/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Destination model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                var destination = db.Destinations.Find(model.Id);
                if (destination == null)
                {
                    return HttpNotFound();
                }

                destination.Name = model.Name;
                destination.Region = model.Region;
                destination.Description = model.Description;
                destination.CategoryId = model.CategoryId;

                if (imageFile != null && imageFile.ContentLength > 0)
                {
                    string fileName = SaveImageFile(imageFile);
                    if (fileName == null)
                    {
                        ModelState.AddModelError("ImagePath", "Invalid image format. Only JPG, JPEG, PNG, and GIF are allowed.");
                        PopulateCategoriesDropDownList(model.CategoryId);
                        return View(model);
                    }

                    DeleteImageFile(destination.ImagePath);
                    destination.ImagePath = fileName;
                }

                db.SaveChanges();
                TempData["SuccessMessage"] = "Destination updated successfully!";
                return RedirectToAction("Index");
            }

            PopulateCategoriesDropDownList(model.CategoryId);
            return View(model);
        }


        // GET: Destinations/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            var destination = db.Destinations.Include("Category").FirstOrDefault(d => d.Id == id);
            if (destination == null)
            {
                return HttpNotFound();
            }
            return View(destination);
        }

        // POST: Destinations/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            var destination = db.Destinations.Find(id);
            if (destination == null)
            {
                return HttpNotFound();
            }

            DeleteImageFile(destination.ImagePath);
            db.Destinations.Remove(destination);
            db.SaveChanges();

            TempData["SuccessMessage"] = "Destination deleted successfully!";
            return RedirectToAction("Index");
        }
        private string SaveImageFile(HttpPostedFileBase imageFile)
        {
            string extension = Path.GetExtension(imageFile.FileName).ToLower();
            string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

            if (!allowedExtensions.Contains(extension))
            {
                return null;
            }

            string fileName = Guid.NewGuid().ToString() + extension;
            string relativePath = "~/Content/Images/Destinations";
            string absolutePath = Server.MapPath(relativePath);

            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }

            string fullPath = Path.Combine(absolutePath, fileName);
            imageFile.SaveAs(fullPath);

            return Url.Content(relativePath + "/" + fileName);
        }

        private void DeleteImageFile(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                try
                {
                    string physicalPath = Server.MapPath(imagePath);
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("Error deleting image: " + ex.Message);
                }
            }
        }

        private void PopulateCategoriesDropDownList(object selectedCategory = null)
        {
            var categories = db.Categories.OrderBy(c => c.Name).ToList();
            ViewBag.CategoryList = new SelectList(categories, "Id", "Name", selectedCategory);
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
}
