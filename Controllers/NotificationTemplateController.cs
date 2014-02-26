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
    public class NotificationTemplateController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /NotificationTemplate/

        public ActionResult Index()
        {
            var notificationtemplates = db.NotificationTemplates;
            return View(notificationtemplates.ToList());
        }

        //
        // GET: /NotificationTemplate/Details/5

        public ActionResult Details(int id = 0)
        {
            NotificationTemplate notificationtemplate = db.NotificationTemplates.Find(id);
            if (notificationtemplate == null)
            {
                Session["FlashMessage"] = "Notification Template not found.";
                return RedirectToAction("Index");
            }
            return View(notificationtemplate);
        }

        //
        // GET: /NotificationTemplate/Create

        public ActionResult Create()
        {
            ViewBag.type_id = new SelectList(db.NotificationTypes, "id", "name");
            return View();
        }

        //
        // POST: /NotificationTemplate/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(NotificationTemplate notificationtemplate)
        {
            if (ModelState.IsValid)
            {
                db.NotificationTemplates.Add(notificationtemplate);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.type_id = new SelectList(db.NotificationTypes, "id", "name", notificationtemplate.type_id);
            return View(notificationtemplate);
        }

        //
        // GET: /NotificationTemplate/Edit/5

        public ActionResult Edit(int id = 0)
        {
            NotificationTemplate notificationtemplate = db.NotificationTemplates.Find(id);
            if (notificationtemplate == null)
            {
                Session["FlashMessage"] = "Notification Template not found.";
                return RedirectToAction("Index");
            }
            ViewBag.type_id = new SelectList(db.NotificationTypes, "id", "name", notificationtemplate.type_id);
            return View(notificationtemplate);
        }

        //
        // POST: /NotificationTemplate/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(NotificationTemplate notificationtemplate)
        {
            if (ModelState.IsValid)
            {
                db.Entry(notificationtemplate).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.type_id = new SelectList(db.NotificationTypes, "id", "name", notificationtemplate.type_id);
            return View(notificationtemplate);
        }

        //
        // GET: /NotificationTemplate/Delete/5

        public ActionResult Delete(int id = 0)
        {
            NotificationTemplate notificationtemplate = db.NotificationTemplates.Find(id);
            if (notificationtemplate == null)
            {
                Session["FlashMessage"] = "Notification Template not found.";
                return RedirectToAction("Index");
            }
            if (notificationtemplate.NotificationTypes != null)
            {
                Session["FlashMessage"] = "Notification Template is attached to existing Notification Template(s).";
                return RedirectToAction("Index");
            }
            return View(notificationtemplate);
        }

        //
        // POST: /NotificationTemplate/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NotificationTemplate notificationtemplate = db.NotificationTemplates.Find(id);
            notificationtemplate.Notifications.Clear();
            db.NotificationTemplates.Remove(notificationtemplate);
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