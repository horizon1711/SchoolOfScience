using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;

namespace SchoolOfScience.Controllers
{
    [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
    public class AppointmentVenueController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /AppointmentVenue/

        public ActionResult Index()
        {
            return View(db.AppointmentVenues.ToList());
        }

        //
        // GET: /AppointmentVenue/Details/5

        public ActionResult Details(int id = 0)
        {
            AppointmentVenue appointmentvenue = db.AppointmentVenues.Find(id);
            if (appointmentvenue == null)
            {
                Session["FlashMessage"] = "Appointment Venue not found.";
                return RedirectToAction("Index");
            }
            return View(appointmentvenue);
        }

        //
        // GET: /AppointmentVenue/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /AppointmentVenue/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppointmentVenue appointmentvenue)
        {
            if (ModelState.IsValid)
            {
                db.AppointmentVenues.Add(appointmentvenue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(appointmentvenue);
        }

        //
        // GET: /AppointmentVenue/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AppointmentVenue appointmentvenue = db.AppointmentVenues.Find(id);
            if (appointmentvenue == null)
            {
                Session["FlashMessage"] = "Appointment Venue not found.";
                return RedirectToAction("Index");
            }
            return View(appointmentvenue);
        }

        //
        // POST: /AppointmentVenue/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentVenue appointmentvenue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointmentvenue).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(appointmentvenue);
        }

        //
        // GET: /AppointmentVenue/Delete/5

        public ActionResult Delete(int id = 0)
        {
            AppointmentVenue appointmentvenue = db.AppointmentVenues.Find(id);
            if (appointmentvenue == null)
            {
                Session["FlashMessage"] = "Appointment Venue not found.";
                return RedirectToAction("Index");
            }
            if (appointmentvenue.Appointments != null)
            {
                Session["FlashMessage"] = "Appointment Venue is attached to existing Appointment(s).";
                return RedirectToAction("Index");
            }
            return View(appointmentvenue);
        }

        //
        // POST: /AppointmentVenue/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AppointmentVenue appointmentvenue = db.AppointmentVenues.Find(id);
            db.AppointmentVenues.Remove(appointmentvenue);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}