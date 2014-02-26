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
    public class StudentExperienceController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /StudentExperience/

        //public ActionResult Index()
        //{
        //    var studentexperiences = db.StudentExperiences.Include(s => s.StudentExperienceType).Include(s => s.StudentProfile);
        //    return View(studentexperiences.ToList());
        //}

        ////
        //// GET: /StudentExperience/Details/5

        //public ActionResult Details(int id = 0)
        //{
        //    StudentExperience studentexperience = db.StudentExperiences.Find(id);
        //    if (studentexperience == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(studentexperience);
        //}

        //
        // GET: /StudentExperience/Create

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Create(int id = 0)
        {
            var type = db.StudentExperienceTypes.Find(id);
            var student = db.StudentProfiles.Find(User.Identity.Name);
            if (student == null || type == null)
            {
                return HttpNotFound("Student Profile or Experience Type not found.");
            }
            StudentExperience studentexperience = new StudentExperience
            {
                type_id = id,
                student_id = student.id,
                StudentProfile = student,
                StudentExperienceType = type
            };
            PrepareList();
            return View(studentexperience);
        }

        //
        // POST: /StudentExperience/Create

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentExperience studentexperience)
        {
            if (ModelState.IsValid)
            {
                db.StudentExperiences.Add(studentexperience);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return HttpNotFound("Failed to add experience.<br/><br/>" + e.Message);
                }
            }
            return RedirectToAction("MyExperience", "StudentProfile", new { student_id = studentexperience.student_id });
        }

        //
        // GET: /StudentExperience/Edit/5

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Edit(int id = 0)
        {
            StudentExperience studentexperience = db.StudentExperiences.ToList().Where(p => p.id == id && p.student_id.ToString() == User.Identity.Name).SingleOrDefault();
            if (studentexperience == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            PrepareList();
            return View(studentexperience);
        }

        //
        // POST: /StudentExperience/Edit/5

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentExperience studentexperience)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studentexperience).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("MyExperience", "StudentProfile", new { student_id = studentexperience.student_id });
        }

        //
        // GET: /StudentExperience/Delete/5

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Delete(int id = 0)
        {
            StudentExperience studentexperience = db.StudentExperiences.ToList().Where(p => p.id == id && p.student_id.ToString() == User.Identity.Name).SingleOrDefault();
            var student_id = studentexperience.student_id;
            if (studentexperience == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            db.StudentExperiences.Remove(studentexperience);
            db.SaveChanges();
            return RedirectToAction("MyExperience", "StudentProfile", new { student_id = student_id });
        }

        //
        // POST: /StudentExperience/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    StudentExperience studentexperience = db.StudentExperiences.Find(id);
        //    db.StudentExperiences.Remove(studentexperience);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        protected void PrepareList()
        {
            List<SelectListItem> smList = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                smList.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }
            ViewBag.startmonthList = new SelectList(smList, "Value", "Text");

            List<SelectListItem> syList = new List<SelectListItem>();
            for (int i = DateTime.Now.AddYears(-10).Year; i <= DateTime.Now.Year; i++)
            {
                syList.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }
            ViewBag.startyearList = new SelectList(syList, "Value", "Text");

            List<SelectListItem> mList = new List<SelectListItem>();
            for (int i = 1; i <= 12; i++)
            {
                mList.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }
            mList.Add(new SelectListItem
            {
                Text = "Present",
                Value = "-1"
            });
            ViewBag.monthList = new SelectList(mList, "Value", "Text");

            List<SelectListItem> yList = new List<SelectListItem>();
            for (int i = DateTime.Now.AddYears(-10).Year; i <= DateTime.Now.Year; i++)
            {
                yList.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }
            yList.Add(new SelectListItem
            {
                Text = "Present",
                Value = "-1"
            });
            ViewBag.yearList = new SelectList(yList, "Value", "Text");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}