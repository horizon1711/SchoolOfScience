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
    public class AppointmentStatusController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /AppointmentStatus/

        public ActionResult Index()
        {
            return View(db.AppointmentStatus.ToList());
        }

        //
        // GET: /AppointmentStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            AppointmentStatus appointmentstatus = db.AppointmentStatus.Find(id);
            if (appointmentstatus == null)
            {
                Session["FlashMessage"] = "Appointment Status not found.";
                return RedirectToAction("Index");
            }
            return View(appointmentstatus);
        }

        //
        // GET: /AppointmentStatus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /AppointmentStatus/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppointmentStatus appointmentstatus)
        {
            if (ModelState.IsValid)
            {
                db.AppointmentStatus.Add(appointmentstatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(appointmentstatus);
        }

        //
        // GET: /AppointmentStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AppointmentStatus appointmentstatus = db.AppointmentStatus.Find(id);
            if (appointmentstatus == null)
            {
                Session["FlashMessage"] = "Appointment Status not found.";
                return RedirectToAction("Index");
            }
            return View(appointmentstatus);
        }

        //
        // POST: /AppointmentStatus/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentStatus appointmentstatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointmentstatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(appointmentstatus);
        }

        //
        // GET: /AppointmentStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            AppointmentStatus appointmentstatus = db.AppointmentStatus.Find(id);
            if (appointmentstatus == null)
            {
                Session["FlashMessage"] = "Appointment Status not found.";
                return RedirectToAction("Index");
            }
            if (appointmentstatus.Appointments != null)
            {
                Session["FlashMessage"] = "Appointment Status is attached to existing Appointment(s).";
                return RedirectToAction("Index");
            }
            return View(appointmentstatus);
        }

        //
        // POST: /AppointmentStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AppointmentStatus appointmentstatus = db.AppointmentStatus.Find(id);
            db.AppointmentStatus.Remove(appointmentstatus);
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