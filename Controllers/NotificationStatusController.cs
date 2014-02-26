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
    public class NotificationStatusController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /NotificationStatus/

        public ActionResult Index()
        {
            return View(db.NotificationStatus.ToList());
        }

        //
        // GET: /NotificationStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            NotificationStatus notificationstatus = db.NotificationStatus.Find(id);
            if (notificationstatus == null)
            {
                Session["FlashMessage"] = "Notification Status not found.";
                return RedirectToAction("Index");
            }
            return View(notificationstatus);
        }

        //
        // GET: /NotificationStatus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /NotificationStatus/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NotificationStatus notificationstatus)
        {
            if (ModelState.IsValid)
            {
                db.NotificationStatus.Add(notificationstatus);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(notificationstatus);
        }

        //
        // GET: /NotificationStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            NotificationStatus notificationstatus = db.NotificationStatus.Find(id);
            if (notificationstatus == null)
            {
                Session["FlashMessage"] = "Notification Status not found.";
                return RedirectToAction("Index");
            }
            return View(notificationstatus);
        }

        //
        // POST: /NotificationStatus/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NotificationStatus notificationstatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(notificationstatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(notificationstatus);
        }

        //
        // GET: /NotificationStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            NotificationStatus notificationstatus = db.NotificationStatus.Find(id);
            if (notificationstatus == null)
            {
                Session["FlashMessage"] = "Notification Status not found.";
                return RedirectToAction("Index");
            }
            if (notificationstatus.Notifications != null)
            {
                Session["FlashMessage"] = "Notification Status is attached to existing Notification(s).";
                return RedirectToAction("Index");
            }
            return View(notificationstatus);
        }

        //
        // POST: /NotificationStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NotificationStatus notificationstatus = db.NotificationStatus.Find(id);
            db.NotificationStatus.Remove(notificationstatus);
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