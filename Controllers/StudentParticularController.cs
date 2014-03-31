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
    public class StudentParticularController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /StudentParticular/

        //public ActionResult Index()
        //{
        //    var studentparticulars = db.StudentParticulars.Include(s => s.StudentParticularType).Include(s => s.StudentProfile);
        //    return View(studentparticulars.ToList());
        //}

        ////
        //// GET: /StudentParticular/Details/5

        //public ActionResult Details(int id = 0)
        //{
        //    StudentParticular studentparticular = db.StudentParticulars.Find(id);
        //    if (studentparticular == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(studentparticular);
        //}

        //
        // GET: /StudentParticular/Create

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Create(int id = 0)
        {
            var type = db.StudentParticularTypes.Find(id);
            var student = db.StudentProfiles.Find(User.Identity.Name);
            if (student == null || type == null)
            {
                return HttpNotFound("Student Profile or Particular Type not found.");
            }
            StudentParticular particular = new StudentParticular
            {
                type_id = id,
                student_id = student.id,
                StudentProfile = student,
                StudentParticularType = type
            };
            return View(particular);
        }

        //
        // POST: /StudentParticular/Create

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentParticular studentparticular)
        {
            if (ModelState.IsValid)
            {
                db.StudentParticulars.Add(studentparticular);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return HttpNotFound("Failed to add particular.<br/><br/>" + e.Message);
                }
            }
            return RedirectToAction("MyParticular", "StudentProfile", new { student_id = studentparticular.student_id });
        }

        ////201403280253 fai: not used code. as language changed to free text
        //// GET: /StudentParticular/EditLanguage?student_id=

        //[Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        //public ActionResult EditLanguage(int id = 0)
        //{
        //    var student = db.StudentProfiles.Find(User.Identity.Name);
        //    var type = db.StudentParticularTypes.Find(id);
        //    if (student == null || type == null)
        //    {
        //        return HttpNotFound("Student Profile or Particular Type not found.");
        //    }

        //    List<StudentParticular> particulars = student.StudentParticulars.Where(p => p.student_id == student.id && p.type_id == id).ToList();
        //    if (particulars == null || particulars.Count() == 0)
        //    {
        //        particulars.Add(new StudentParticular
        //        {
        //            student_id = student.id,
        //            type_id = id,
        //            StudentProfile = student,
        //            StudentParticularType = type,
        //            name = "English"
        //        });
        //        particulars.Add(new StudentParticular
        //        {
        //            student_id = student.id,
        //            type_id = id,
        //            StudentProfile = student,
        //            StudentParticularType = type,
        //            name = "Putonghua"
        //        });
        //        particulars.Add(new StudentParticular
        //        {
        //            student_id = student.id,
        //            type_id = id,
        //            StudentProfile = student,
        //            StudentParticularType = type,
        //            name = "Cantonese"
        //        });
        //    }

        //    //add a blank particular for new other language
        //    particulars.Add(new StudentParticular
        //    {
        //        student_id = student.id,
        //        type_id = id,
        //        StudentProfile = student,
        //        StudentParticularType = type,
        //        name = ""
        //    });

        //    List<SelectListItem> descriptionList = new List<SelectListItem>();
        //    descriptionList.Add(new SelectListItem { Text = "Native", Value = "Native" });
        //    descriptionList.Add(new SelectListItem { Text = "Fluent", Value = "Fluent" });
        //    descriptionList.Add(new SelectListItem { Text = "Fair", Value = "Fair" });
        //    descriptionList.Add(new SelectListItem { Text = "Basic", Value = "Basic" });
        //    descriptionList.Add(new SelectListItem { Text = "N/A", Value = "N/A" });
        //    ViewBag.descriptionList = descriptionList;

        //    ViewBag.student_id = student.id;

        //    return View(particulars);
        //}

        ////
        //// POST: /StudentParticular/EditLanguage

        //[HttpPost]
        //[Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        //[ValidateAntiForgeryToken]
        //public ActionResult EditLanguage(List<StudentParticular> particulars)
        //{
        //    foreach (var particular in particulars)
        //    {
        //        if (particular.id == 0)
        //        {
        //            if (!String.IsNullOrEmpty(particular.name))
        //            {
        //                db.StudentParticulars.Add(particular);
        //            }
        //        }
        //        else
        //        {
        //            db.Entry(particular).State = EntityState.Modified;
        //        }
        //    }
        //    db.SaveChanges();
        //    return RedirectToAction("MyParticular", "StudentProfile", new { student_id = User.Identity.Name });
        //}

        //
        // GET: /StudentParticular/Edit/5

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Edit(int id = 0)
        {
            StudentParticular studentparticular = db.StudentParticulars.ToList().Where(p => p.id == id && p.student_id.ToString() == User.Identity.Name).SingleOrDefault();
            if (studentparticular == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            return View(studentparticular);
        }

        //
        // POST: /StudentParticular/Edit/5

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentParticular studentparticular)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studentparticular).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(studentparticular);
        }

        //
        // GET: /StudentParticular/Delete/5

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Delete(int id = 0)
        {
            StudentParticular studentparticular = db.StudentParticulars.ToList().Where(p => p.id == id && p.student_id.ToString() == User.Identity.Name).SingleOrDefault();
            var student_id = studentparticular.student_id;
            if (studentparticular == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            db.StudentParticulars.Remove(studentparticular);
            db.SaveChanges();
            return RedirectToAction("MyParticular", "StudentProfile", new { student_id = student_id });
        }

        //
        // POST: /StudentParticular/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    StudentParticular studentparticular = db.StudentParticulars.Find(id);
        //    db.StudentParticulars.Remove(studentparticular);
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