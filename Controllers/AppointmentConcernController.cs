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
    public class AppointmentConcernController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /AppointmentConcern/

        public ActionResult Index()
        {
            return View(db.AppointmentConcerns.ToList());
        }

        //
        // GET: /AppointmentConcern/Details/5

        public ActionResult Details(int id = 0)
        {
            AppointmentConcern appointmentconcern = db.AppointmentConcerns.Find(id);
            if (appointmentconcern == null)
            {
                Session["FlashMessage"] = "Appointment Concern not found.";
                return RedirectToAction("Index");
            }
            return View(appointmentconcern);
        }

        //
        // GET: /AppointmentConcern/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /AppointmentConcern/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppointmentConcern appointmentconcern)
        {
            if (ModelState.IsValid)
            {
                db.AppointmentConcerns.Add(appointmentconcern);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(appointmentconcern);
        }

        //
        // GET: /AppointmentConcern/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AppointmentConcern appointmentconcern = db.AppointmentConcerns.Find(id);
            if (appointmentconcern == null)
            {
                Session["FlashMessage"] = "Appointment Concern not found.";
                return RedirectToAction("Index");
            }
            return View(appointmentconcern);
        }

        //
        // POST: /AppointmentConcern/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentConcern appointmentconcern)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointmentconcern).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(appointmentconcern);
        }

        //
        // GET: /AppointmentConcern/Delete/5

        public ActionResult Delete(int id = 0)
        {
            AppointmentConcern appointmentconcern = db.AppointmentConcerns.Find(id);
            if (appointmentconcern == null)
            {
                Session["FlashMessage"] = "Appointment Concern not found.";
                return RedirectToAction("Index");
            }
            if (appointmentconcern.Appointments != null && appointmentconcern.Appointments.Count() > 0)
            {
                Session["FlashMessage"] = "Appointment Concern is attached to existing Appointment(s).";
                return RedirectToAction("Index");
            }
            return View(appointmentconcern);
        }

        //
        // POST: /AppointmentConcern/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AppointmentConcern appointmentconcern = db.AppointmentConcerns.Find(id);
            appointmentconcern.Appointments.Clear();
            db.AppointmentConcerns.Remove(appointmentconcern);
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