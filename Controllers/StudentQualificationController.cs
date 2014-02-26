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
    public class StudentQualificationController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /StudentQualification/

        //public ActionResult Index()
        //{
        //    var studentqualifications = db.StudentQualifications.Include(s => s.StudentProfile);
        //    return View(studentqualifications.ToList());
        //}

        //
        // GET: /StudentQualification/Details/5

        //public ActionResult Details(int id = 0)
        //{
        //    StudentQualification studentqualification = db.StudentQualifications.Find(id);
        //    if (studentqualification == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(studentqualification);
        //}

        //
        // GET: /StudentExperience/Create

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Create()
        {
            var student = db.StudentProfiles.Find(User.Identity.Name);
            if (student == null)
            {
                return HttpNotFound("Student Profile not found.");
            }
            StudentQualification studentqualification = new StudentQualification
            {
                student_id = student.id,
                StudentProfile = student,
                award_date = DateTime.Now.Date
            };
            return View(studentqualification);
        }

        //
        // POST: /StudentExperience/Create

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentQualification studentqualification)
        {
            if (ModelState.IsValid)
            {
                db.StudentQualifications.Add(studentqualification);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return HttpNotFound("Failed to add qualification.<br/><br/>" + e.Message);
                }
            }
            return RedirectToAction("MyQualification", "StudentProfile", new { student_id = studentqualification.student_id });
        }

        //
        // GET: /StudentQualification/Edit/5

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Edit(int id = 0)
        {
            StudentQualification studentqualification = db.StudentQualifications.ToList().Where(p => p.id == id && p.student_id.ToString() == User.Identity.Name).SingleOrDefault();
            if (studentqualification == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            return View(studentqualification);
        }

        //
        // POST: /StudentQualification/Edit/5

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentQualification studentqualification)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studentqualification).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("MyQualification", "StudentProfile", new { student_id = studentqualification.student_id });
        }

        //
        // GET: /StudentQualification/Delete/5

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Delete(int id = 0)
        {
            StudentQualification studentqualification = db.StudentQualifications.ToList().Where(p => p.id == id && p.student_id.ToString() == User.Identity.Name).SingleOrDefault();
            var student_id = studentqualification.student_id;
            if (studentqualification == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            db.StudentQualifications.Remove(studentqualification);
            db.SaveChanges();
            return RedirectToAction("MyQualification", "StudentProfile", new { student_id = student_id });
        }

        //
        // POST: /StudentQualification/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    StudentQualification studentqualification = db.StudentQualifications.Find(id);
        //    db.StudentQualifications.Remove(studentqualification);
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