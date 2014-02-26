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
    public class StudentExperienceTypeController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /StudentExperienceType/

        public ActionResult Index()
        {
            return View(db.StudentExperienceTypes.ToList());
        }

        //
        // GET: /StudentExperienceType/Details/5

        public ActionResult Details(int id = 0)
        {
            StudentExperienceType studentexperiencetype = db.StudentExperienceTypes.Find(id);
            if (studentexperiencetype == null)
            {
                return HttpNotFound();
            }
            return View(studentexperiencetype);
        }

        //
        // GET: /StudentExperienceType/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /StudentExperienceType/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentExperienceType studentexperiencetype)
        {
            if (ModelState.IsValid)
            {
                db.StudentExperienceTypes.Add(studentexperiencetype);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(studentexperiencetype);
        }

        //
        // GET: /StudentExperienceType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            StudentExperienceType studentexperiencetype = db.StudentExperienceTypes.Find(id);
            if (studentexperiencetype == null)
            {
                return HttpNotFound();
            }
            return View(studentexperiencetype);
        }

        //
        // POST: /StudentExperienceType/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentExperienceType studentexperiencetype)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studentexperiencetype).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(studentexperiencetype);
        }

        //
        // GET: /StudentExperienceType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            StudentExperienceType studentexperiencetype = db.StudentExperienceTypes.Find(id);
            if (studentexperiencetype == null)
            {
                return HttpNotFound();
            }
            return View(studentexperiencetype);
        }

        //
        // POST: /StudentExperienceType/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            StudentExperienceType studentexperiencetype = db.StudentExperienceTypes.Find(id);
            db.StudentExperienceTypes.Remove(studentexperiencetype);
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