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
    public class InterviewStatusController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /InterviewStatus/

        public ActionResult Index()
        {
            return View(db.InterviewStatus.ToList());
        }

        //
        // GET: /InterviewStatus/Details/5

        public ActionResult Details(int id = 0)
        {
            InterviewStatus interviewstatus = db.InterviewStatus.Find(id);
            if (interviewstatus == null)
            {
                Session["FlashMessage"] = "Interview Status not found.";
                return RedirectToAction("Index");
            }
            return View(interviewstatus);
        }

        //
        // GET: /InterviewStatus/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /InterviewStatus/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InterviewStatus interviewstatus)
        {
            if (ModelState.IsValid)
            {
                db.InterviewStatus.Add(interviewstatus);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create status." + e.Message;
                    return View(interviewstatus);
                }
                return RedirectToAction("Index");
            }

            return View(interviewstatus);
        }

        //
        // GET: /InterviewStatus/Edit/5

        public ActionResult Edit(int id = 0)
        {
            InterviewStatus interviewstatus = db.InterviewStatus.Find(id);
            if (interviewstatus == null)
            {
                Session["FlashMessage"] = "Interview Status not found.";
                return RedirectToAction("Index");
            }
            return View(interviewstatus);
        }

        //
        // POST: /InterviewStatus/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(InterviewStatus interviewstatus)
        {
            if (ModelState.IsValid)
            {
                db.Entry(interviewstatus).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to edit status." + e.Message;
                    return View(interviewstatus);
                }
                return RedirectToAction("Index");
            }
            return View(interviewstatus);
        }

        //
        // GET: /InterviewStatus/Delete/5

        public ActionResult Delete(int id = 0)
        {
            InterviewStatus interviewstatus = db.InterviewStatus.Find(id);
            if (interviewstatus == null)
            {
                Session["FlashMessage"] = "Interview Status not found.";
                return RedirectToAction("Index");
            }
            if (interviewstatus.Interviews != null)
            {
                Session["FlashMessage"] = "Interview Status is attached to existing Interview(s).";
                return RedirectToAction("Index");
            }
            return View(interviewstatus);
        }

        //
        // POST: /InterviewStatus/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InterviewStatus interviewstatus = db.InterviewStatus.Find(id);
            db.InterviewStatus.Remove(interviewstatus);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete status." + e.Message;
                return View(interviewstatus);
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