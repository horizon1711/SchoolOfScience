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
    public class InterviewVenueController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /InterviewVenue/

        public ActionResult Index()
        {
            return View(db.InterviewVenues.ToList());
        }

        //
        // GET: /InterviewVenue/Details/5

        public ActionResult Details(int id = 0)
        {
            InterviewVenue interviewvenue = db.InterviewVenues.Find(id);
            if (interviewvenue == null)
            {
                Session["FlashMessage"] = "Interview Venue not found.";
                return RedirectToAction("Index");
            }
            return View(interviewvenue);
        }

        //
        // GET: /InterviewVenue/Create

        public ActionResult Create()
        {
            var model = new InterviewVenue();
            model.status = true;
            return View(model);
        }

        //
        // POST: /InterviewVenue/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InterviewVenue interviewvenue)
        {
            if (ModelState.IsValid)
            {
                db.InterviewVenues.Add(interviewvenue);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(interviewvenue);
        }

        //
        // GET: /InterviewVenue/Edit/5

        public ActionResult Edit(int id = 0)
        {
            InterviewVenue interviewvenue = db.InterviewVenues.Find(id);
            if (interviewvenue == null)
            {
                Session["FlashMessage"] = "Interview Venue not found.";
                return RedirectToAction("Index");
            }
            return View(interviewvenue);
        }

        //
        // POST: /InterviewVenue/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(InterviewVenue interviewvenue)
        {
            if (ModelState.IsValid)
            {
                db.Entry(interviewvenue).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(interviewvenue);
        }

        //
        // GET: /InterviewVenue/Delete/5

        public ActionResult Delete(int id = 0)
        {
            InterviewVenue interviewvenue = db.InterviewVenues.Find(id);
            
            if (interviewvenue == null)
            {
                Session["FlashMessage"] = "Interview Venue not found.";
                return RedirectToAction("Index");
            }
            if (interviewvenue.Interviews != null)
            {
                Session["FlashMessage"] = "Interview Venue is attached to existing Interview(s).";
                return RedirectToAction("Index");
            }
            return View(interviewvenue);
        }

        //
        // POST: /InterviewVenue/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            InterviewVenue interviewvenue = db.InterviewVenues.Find(id);
            db.InterviewVenues.Remove(interviewvenue);
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