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
    public class NominationStatusController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /NominationStatus/

        public ActionResult Index()
        {
            return View(db.NominationStatus.ToList());
        }

        //
        // GET: /NominationStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            NominationStatus nominationstatus = db.NominationStatus.Find(id);
            if (nominationstatus == null)
            {
                return HttpNotFound();
            }
            return View(nominationstatus);
        }

        //
        // GET: /NominationStatus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /NominationStatus/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NominationStatus nominationstatus)
        {
            if (ModelState.IsValid)
            {
                db.NominationStatus.Add(nominationstatus);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create status." + e.Message;
                    return View(nominationstatus);
                }
                return RedirectToAction("Index");
            }

            return View(nominationstatus);
        }

        //
        // GET: /NominationStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            NominationStatus nominationstatus = db.NominationStatus.Find(id);
            if (nominationstatus == null)
            {
                return HttpNotFound();
            }
            return View(nominationstatus);
        }

        //
        // POST: /NominationStatus/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NominationStatus nominationstatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(nominationstatus).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to edit status." + e.Message;
                    return View(nominationstatus);
                }
                return RedirectToAction("Index");
            }
            return View(nominationstatus);
        }

        //
        // GET: /NominationStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            NominationStatus nominationstatus = db.NominationStatus.Find(id);
            if (nominationstatus == null)
            {
                return HttpNotFound();
            }
            return View(nominationstatus);
        }

        //
        // POST: /NominationStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            NominationStatus nominationstatus = db.NominationStatus.Find(id);
            db.NominationStatus.Remove(nominationstatus);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete status." + e.Message;
                return View(nominationstatus);
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