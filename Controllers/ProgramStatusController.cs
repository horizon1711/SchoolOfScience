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
    public class ProgramStatusController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /ProgramStatus/

        public ActionResult Index()
        {
            return View(db.ProgramStatus.ToList());
        }

        //
        // GET: /ProgramStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            ProgramStatus programstatus = db.ProgramStatus.Find(id);
            if (programstatus == null)
            {
                Session["FlashMessage"] = "Program Status not found.";
                return RedirectToAction("Index");
            }
            return View(programstatus);
        }

        //
        // GET: /ProgramStatus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /ProgramStatus/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProgramStatus programstatus)
        {
            if (ModelState.IsValid)
            {
                db.ProgramStatus.Add(programstatus);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create status." + e.Message;
                    return View(programstatus);
                }
                return RedirectToAction("Index");
            }

            return View(programstatus);
        }

        //
        // GET: /ProgramStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ProgramStatus programstatus = db.ProgramStatus.Find(id);
            if (programstatus == null)
            {
                Session["FlashMessage"] = "Program Status not found.";
                return RedirectToAction("Index");
            }
            return View(programstatus);
        }

        //
        // POST: /ProgramStatus/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProgramStatus programstatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(programstatus).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to update status." + e.Message;
                    return View(programstatus);
                }
                return RedirectToAction("Index");
            }
            return View(programstatus);
        }

        //
        // GET: /ProgramStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            ProgramStatus programstatus = db.ProgramStatus.Find(id);
            if (programstatus == null)
            {
                Session["FlashMessage"] = "Program Status not found.";
                return RedirectToAction("Index");
            }
            if (programstatus.Programs != null)
            {
                Session["FlashMessage"] = "Program Status is attached to existing Program(s).";
                return RedirectToAction("Index");
            }
            return View(programstatus);
        }

        //
        // POST: /ProgramStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProgramStatus programstatus = db.ProgramStatus.Find(id);
            db.ProgramStatus.Remove(programstatus);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete status." + e.Message;
                return View("Delete", programstatus);
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