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
    public class ApplicationStatusController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /ApplicationStatus/

        public ActionResult Index()
        {
            return View(db.ApplicationStatus.ToList());
        }

        //
        // GET: /ApplicationStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            ApplicationStatus applicationstatus = db.ApplicationStatus.Find(id);
            if (applicationstatus == null)
            {
                return HttpNotFound();
            }
            return View(applicationstatus);
        }

        //
        // GET: /ApplicationStatus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ApplicationStatus/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApplicationStatus applicationstatus)
        {
            if (ModelState.IsValid)
            {
                db.ApplicationStatus.Add(applicationstatus);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create status." + e.Message;
                    return View(applicationstatus);
                }
                return RedirectToAction("Index");
            }

            return View(applicationstatus);
        }

        //
        // GET: /ApplicationStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ApplicationStatus applicationstatus = db.ApplicationStatus.Find(id);
            if (applicationstatus == null)
            {
                return HttpNotFound();
            }
            return View(applicationstatus);
        }

        //
        // POST: /ApplicationStatus/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationStatus applicationstatus)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(applicationstatus).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to edit status." + e.Message;
                    return View(applicationstatus);
                }
                return RedirectToAction("Index");
            }
            return View(applicationstatus);
        }

        //
        // GET: /ApplicationStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            ApplicationStatus applicationstatus = db.ApplicationStatus.Find(id);
            if (applicationstatus == null)
            {
                return HttpNotFound();
            }
            return View(applicationstatus);
        }

        //
        // POST: /ApplicationStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ApplicationStatus applicationstatus = db.ApplicationStatus.Find(id);
            db.ApplicationStatus.Remove(applicationstatus);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete status." + e.Message;
                return View("Delete", applicationstatus);
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}