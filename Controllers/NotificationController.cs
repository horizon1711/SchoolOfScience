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

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index(int notification_id = 0)
        {
            ViewBag.NotificationTypeList = new SelectList(db.NotificationTypes, "id", "name");
            ViewBag.NotificationStatusList = new SelectList(db.NotificationStatus, "id", "name");
            var notifications = db.Notifications.OrderByDescending(n => n.modified).Take(100);
            if (notification_id != 0)
            {
                ViewBag.highlight_id = notification_id;
            }
            return View(notifications.ToList());
        }

        //
        // POST: /Notification/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index(FormCollection Form, int notification_id = 0)
        {
            ViewBag.NotificationTypeList = new SelectList(db.NotificationTypes, "id", "name", Form["notification_type"]);
            ViewBag.NotificationStatusList = new SelectList(db.NotificationStatus, "id", "name", Form["notification_status"]);
            var notifications = db.Notifications;
            if (notification_id != 0)
            {
                ViewBag.highlight_id = notification_id;
            }
            return View(notifications.ToList().Where(n => true
                && (String.IsNullOrEmpty(Form["notification_type"]) || n.type_id.ToString() == Form["notification_type"])
                && (String.IsNullOrEmpty(Form["notification_status"]) || n.status_id.ToString() == Form["notification_status"])
                ));
        }

        //
        // GET: /Notification/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Details(int id = 0)
        {
            Notification notification = db.Notifications.Find(id);
            if (notification == null)
            {
                return HttpNotFound();
            }
            return View(notification);
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Send(int id = 0)
        {
            Notification notification = db.Notifications.Find(id);
            db.Entry(notification).CurrentValues.SetValues(SendNotification(notification));
            db.SaveChanges();
            return RedirectToAction("Index", new { notification_id = id });
        }

        //
        // GET: /Notification/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Notification notification = db.Notifications.Find(id);
            var recipients = db.NotificationRecipients.Where(r => r.notification_id == id);
            recipients.ToList().ForEach(r => db.NotificationRecipients.Remove(r));
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