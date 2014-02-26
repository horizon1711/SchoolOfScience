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
    public class StudentParticularTypeController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /StudentParticularType/

        [Authorize(Roles="Admin,Advising,StudentDevelopment")]
        public ActionResult Index()
        {
            return View(db.StudentParticularTypes.ToList());
        }

        //
        // GET: /StudentParticularType/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Details(int id = 0)
        {
            StudentParticularType studentparticulartype = db.StudentParticularTypes.Find(id);
            if (studentparticulartype == null)
            {
                return HttpNotFound();
            }
            return View(studentparticulartype);
        }

        //
        // GET: /StudentParticularType/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /StudentParticularType/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentParticularType studentparticulartype)
        {
            if (ModelState.IsValid)
            {
                db.StudentParticularTypes.Add(studentparticulartype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(studentparticulartype);
        }

        //
        // GET: /StudentParticularType/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            StudentParticularType studentparticulartype = db.StudentParticularTypes.Find(id);
            if (studentparticulartype == null)
            {
                return HttpNotFound();
            }
            return View(studentparticulartype);
        }

        //
        // POST: /StudentParticularType/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentParticularType studentparticulartype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studentparticulartype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(studentparticulartype);
        }

        //
        // GET: /StudentParticularType/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0)
        {
            StudentParticularType studentparticulartype = db.StudentParticularTypes.Find(id);
            if (studentparticulartype == null)
            {
                return HttpNotFound();
            }
            return View(studentparticulartype);
        }

        //
        // POST: /StudentParticularType/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StudentParticularType studentparticulartype = db.StudentParticularTypes.Find(id);
            db.StudentParticularTypes.Remove(studentparticulartype);
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