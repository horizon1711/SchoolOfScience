using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using System.Net.Mail;
using WebMatrix.WebData;

namespace SchoolOfScience.Controllers
{
    public class NotificationController : ControllerBase
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /Notification/

        public ActionResult Index()
        {
            var notifications = db.Notifications;
            return View(notifications.ToList());
        }

        //
        // GET: /Notification/Details/5

        public ActionResult Details(int id = 0)
        {
            Notification notification = db.Notifications.Find(id);
            if (notification == null)
            {
                return HttpNotFound();
            }
            return View(notification);
        }

        public ActionResult Send(int id = 0, string returnUrl = null)
        {
            Notification notification = db.Notifications.Find(id);
            SendNotification(notification);
            return RedirectToAction("Index");
        }

        //
        // GET: /Notification/Create

        public ActionResult Create()
        {
            ViewBag.status_id = new SelectList(db.NotificationStatus, "id", "name");
            ViewBag.template_id = new SelectList(db.NotificationTemplates, "id", "name");
            ViewBag.type_id = new SelectList(db.NotificationTypes, "id", "name");
            return View();
        }

        //
        // POST: /Notification/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(Notification notification)
        {
            if (ModelState.IsValid)
            {
                db.Notifications.Add(notification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.status_id = new SelectList(db.NotificationStatus, "id", "name", notification.status_id);
            ViewBag.template_id = new SelectList(db.NotificationTemplates, "id", "name", notification.template_id);
            ViewBag.type_id = new SelectList(db.NotificationTypes, "id", "name", notification.type_id);
            return View(notification);
        }

        //
        // GET: /Notification/Edit/5

        public ActionResult Edit(int id = 0)
        {
            Notification notification = db.Notifications.Find(id);
            if (notification == null)
            {
                return HttpNotFound();
            }
            ViewBag.status_id = new SelectList(db.NotificationStatus, "id", "name", notification.status_id);
            ViewBag.template_id = new SelectList(db.NotificationTemplates, "id", "name", notification.template_id);
            ViewBag.type_id = new SelectList(db.NotificationTypes, "id", "name", notification.type_id);
            return View(notification);
        }

        //
        // POST: /Notification/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Notification notification)
        {
            if (ModelState.IsValid)
            {
                db.Entry(notification).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.status_id = new SelectList(db.NotificationStatus, "id", "name", notification.status_id);
            ViewBag.template_id = new SelectList(db.NotificationTemplates, "id", "name", notification.template_id);
            ViewBag.type_id = new SelectList(db.NotificationTypes, "id", "name", notification.type_id);
            return View(notification);
        }

        //
        // GET: /Notification/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Notification notification = db.Notifications.Find(id);
            if (notification == null)
            {
                return HttpNotFound();
            }
            return View(notification);
        }

        //
        // POST: /Notification/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Notification notification = db.Notifications.Find(id);
            db.Notifications.Remove(notification);
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