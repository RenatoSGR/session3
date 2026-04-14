using System;
using System.IO;
using System.Linq;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : BaseController
    {
        private readonly IWebHostEnvironment _environment;

        public CoursesController(SchoolContext db, INotificationService notificationService, IWebHostEnvironment environment)
            : base(db, notificationService)
        {
            _environment = environment;
        }

        // GET: Courses
        public IActionResult Index()
        {
            var courses = db.Courses.Include(c => c.Department);
            return View(courses.ToList());
        }

        // GET: Courses/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null) return BadRequest();
            var course = db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).SingleOrDefault();
            if (course == null) return NotFound();
            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name");
            return View(new Course());
        }

        // POST: Courses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile? teachingMaterialImage)
        {
            if (ModelState.IsValid)
            {
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    var (error, relativePath) = SaveUploadedFile(teachingMaterialImage, course.CourseID, null);
                    if (error != null)
                    {
                        ModelState.AddModelError("teachingMaterialImage", error);
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }
                    course.TeachingMaterialImagePath = relativePath;
                }

                db.Courses.Add(course);
                db.SaveChanges();
                SendEntityNotification("Course", course.CourseID.ToString(), course.Title, EntityOperation.CREATE);
                return RedirectToAction("Index");
            }

            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null) return BadRequest();
            var course = db.Courses.Find(id);
            if (course == null) return NotFound();
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // POST: Courses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit([Bind("CourseID,Title,Credits,DepartmentID,TeachingMaterialImagePath")] Course course, IFormFile? teachingMaterialImage)
        {
            if (ModelState.IsValid)
            {
                if (teachingMaterialImage != null && teachingMaterialImage.Length > 0)
                {
                    var (error, relativePath) = SaveUploadedFile(teachingMaterialImage, course.CourseID, course.TeachingMaterialImagePath);
                    if (error != null)
                    {
                        ModelState.AddModelError("teachingMaterialImage", error);
                        ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
                        return View(course);
                    }
                    course.TeachingMaterialImagePath = relativePath;
                }

                db.Entry(course).State = EntityState.Modified;
                db.SaveChanges();
                SendEntityNotification("Course", course.CourseID.ToString(), course.Title, EntityOperation.UPDATE);
                return RedirectToAction("Index");
            }
            ViewBag.DepartmentID = new SelectList(db.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null) return BadRequest();
            var course = db.Courses.Include(c => c.Department).Where(c => c.CourseID == id).SingleOrDefault();
            if (course == null) return NotFound();
            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var course = db.Courses.Find(id);
            if (course != null)
            {
                var courseTitle = course.Title;

                if (!string.IsNullOrEmpty(course.TeachingMaterialImagePath))
                {
                    var physicalPath = Path.Combine(_environment.WebRootPath, course.TeachingMaterialImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        try { System.IO.File.Delete(physicalPath); }
                        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Error deleting file: {ex.Message}"); }
                    }
                }

                db.Courses.Remove(course);
                db.SaveChanges();
                SendEntityNotification("Course", id.ToString(), courseTitle, EntityOperation.DELETE);
            }
            return RedirectToAction("Index");
        }

        /// <summary>
        /// Validates, saves the uploaded file and returns (errorMessage, relativePath).
        /// On success errorMessage is null; on failure relativePath is null.
        /// </summary>
        private (string? error, string? relativePath) SaveUploadedFile(IFormFile file, int courseId, string? existingRelativePath)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!Array.Exists(allowedExtensions, e => e == extension))
                return ("Please upload a valid image file (jpg, jpeg, png, gif, bmp).", null);

            if (file.Length > 5 * 1024 * 1024)
                return ("File size must be less than 5MB.", null);

            try
            {
                var uploadsPath = Path.Combine(_environment.WebRootPath, "Uploads", "TeachingMaterials");
                Directory.CreateDirectory(uploadsPath);

                // Delete old file if present
                if (!string.IsNullOrEmpty(existingRelativePath))
                {
                    var oldPath = Path.Combine(_environment.WebRootPath, existingRelativePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);
                }

                var fileName = $"course_{courseId}_{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(stream);

                return (null, $"/Uploads/TeachingMaterials/{fileName}");
            }
            catch (Exception ex)
            {
                return ($"Error uploading file: {ex.Message}", null);
            }
        }
    }
}
