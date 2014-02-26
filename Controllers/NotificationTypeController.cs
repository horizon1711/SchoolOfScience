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
    public class NotificationTypeController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /NotificationType/

        public ActionResult Index()
        {
            return View(db.NotificationTypes.ToList());
        }

        //
        // GET: /NotificationType/Details/5

        public ActionResult Details(int id = 0)
        {
            NotificationType notificationtype = db.NotificationTypes.Find(id);
            if (notificationtype == null)
            {
                return HttpNotFound();
            }
            return View(notificationtype);
        }

        //
        // GET: /NotificationType/Create

        //public ActionResult Create()
        //{
        //    return View();
        //}

        ////
        //// POST: /NotificationType/Create

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(NotificationType notificationtype)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.NotificationTypes.Add(notificationtype);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(notificationtype);
        //}

        //
        // GET: /NotificationType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            NotificationType notificationtype = db.NotificationTypes.Find(id);
            ViewBag.templateList = new SelectList(db.NotificationTemplates.Where(t => t.type_id == notificationtype.id), "id", "name");
            if (notificationtype == null)
            {
                return HttpNotFound();
            }
            return View(notificationtype);
        }

        //
        // POST: /NotificationType/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NotificationType notificationtype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(notificationtype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.templateList = new SelectList(db.NotificationTemplates.Where(t => t.type_id == notificationtype.id), "id", "name");
            return View(notificationtype);
        }

        ////
        //// GET: /NotificationType/Delete/5

        //public ActionResult Delete(int id = 0)
        //{
        //    NotificationType notificationtype = db.NotificationTypes.Find(id);
        //    if (notificationtype == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(notificationtype);
        //}

        ////
        //// POST: /NotificationType/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    NotificationType notificationtype = db.NotificationTypes.Find(id);
        //    db.NotificationTypes.Remove(notificationtype);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}