using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using System.IO;
using LinqToExcel;

namespace SchoolOfScience.Controllers
{
    public class StudentActivityController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /StudentActivity/

        //public ActionResult Index()
        //{
        //    var studentactivities = db.StudentActivities.Include(s => s.StudentProfile);
        //    return View(studentactivities.ToList());
        //}

        ////
        //// GET: /StudentActivity/Details/5

        //public ActionResult Details(int id = 0)
        //{
        //    StudentActivity studentactivity = db.StudentActivities.Find(id);
        //    if (studentactivity == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(studentactivity);
        //}

        //
        // GET: /StudentActivity/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create(string student_id = null, string opener_id = null)
        {
            ViewBag.opener_id = opener_id;
            var student = db.StudentProfiles.Find(student_id);
            if (student == null)
            {
                return HttpNotFound("Student Profile not found.");
            }
            StudentActivity studentactivity = new StudentActivity
            {
                start_date = DateTime.Now,
                end_date = DateTime.Now,
                student_id = student.id,
                StudentProfile = student
            };
            return View(studentactivity);
        }

        //
        // POST: /StudentActivity/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentActivity studentactivity, string opener_id = null)
        {
            if (ModelState.IsValid)
            {
                db.StudentActivities.Add(studentactivity);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return HttpNotFound("Failed to add activity record.<br/><br/>" + e.Message);
                }
            }
            return RedirectToAction("ActivityRecord", "StudentProfile", new { student_id = studentactivity.student_id, opener_id = opener_id });
        }

        //
        // GET: /StudentActivity/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0, string opener_id = null)
        {
            ViewBag.opener_id = opener_id;
            StudentActivity studentactivity = db.StudentActivities.Find(id);
            if (studentactivity == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            return View(studentactivity);
        }

        //
        // POST: /StudentActivity/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentActivity studentactivity, string opener_id = null)
        {
            if (ModelState.IsValid)
            {
                db.Entry(studentactivity).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("ActivityRecord", "StudentProfile", new { student_id = studentactivity.student_id, opener_id = opener_id });
        }

        //
        // GET: /StudentActivity/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0, string opener_id = null)
        {
            StudentActivity studentactivity = db.StudentActivities.Find(id);
            var student_id = studentactivity.student_id;
            if (studentactivity == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            db.StudentActivities.Remove(studentactivity);
            db.SaveChanges();
            return RedirectToAction("ActivityRecord", "StudentProfile", new { student_id = student_id, opener_id = opener_id });
        }

        //
        // POST: /StudentActivity/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    StudentActivity studentactivity = db.StudentActivities.Find(id);
        //    db.StudentActivities.Remove(studentactivity);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //
        // GET: /StudentActivity/Import/
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Import()
        {
            //clear files uploaded but not used
            if (Directory.Exists(Server.MapPath("~/App_Data/Import/StudentActivity/" + User.Identity.Name)))
            {
                var files = Directory.GetFiles(Server.MapPath("~/App_Data/Import/StudentActivity/" + User.Identity.Name));
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }
            }
            return View();
        }

        //
        // POST: /StudentActivity/Import/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Import(string filename)
        {
            ViewBag.filename = filename;
            var filepath = Server.MapPath("~/App_Data/Import/StudentActivity/" + User.Identity.Name + "/" + filename);
            if (!System.IO.File.Exists(filepath))
            {
                Session["FlashMessage"] = "File not found.";
                return View();
            }
            try
            {
                int count = 0;
                var excel = new ExcelQueryFactory(filepath);
                var sheetnames = excel.GetWorksheetNames();
                var activities = from c in excel.Worksheet<StudentActivity>(sheetnames.First())
                                 select c;
                foreach (var activity in activities)
                {
                    var student = db.StudentProfiles.Find(activity.student_id);
                    if (student != null)
                    {
                        db.StudentActivities.Add(activity);
                        count++;
                    }
                }
                db.SaveChanges();
                Session["FlashMessage"] = count + " record(s) successfully imported.";
                //clear files uploaded after import
                if (Directory.Exists(Server.MapPath("~/App_Data/Import/StudentActivity/" + User.Identity.Name)))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Import/StudentActivity/" + User.Identity.Name));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to import activities from excel file. <br/><br/>" + HttpUtility.JavaScriptStringEncode(e.Message);
                return View();
            }
            return RedirectToAction("Import");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}