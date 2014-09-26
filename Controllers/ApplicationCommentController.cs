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
    public class ApplicationCommentController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /ApplicationComment/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index(int application_id = 0, string student_id = null)
        {
            var application = db.Applications.Find(application_id);
            if (application == null)
            {
                return HttpNotFound("Application not found.");
            }
            var applicationcomments = db.ApplicationComments.Where(c => c.application_id == application_id && (student_id == null || c.Application.student_id == student_id));
            ViewBag.application = application;
            return View(applicationcomments.ToList());
        }

        //
        // GET: /ApplicationComment/Details/5

        //public ActionResult Details(int id = 0)
        //{
        //    ApplicationComment applicationcomment = db.ApplicationComments.Find(id);
        //    if (applicationcomment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(applicationcomment);
        //}

        //
        // GET: /ApplicationComment/Create/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create(int application_id = 0)
        {
            var application = db.Applications.SingleOrDefault(i => i.id == application_id);
            if (application == null)
            {
                return HttpNotFound("Application not found.");
            }
            ApplicationComment applicationcomment = new ApplicationComment
            {
                application_id = application_id,
                Application = application
            };

            return View(applicationcomment);
        }

        //
        // POST: /ApplicationComment/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApplicationComment applicationcomment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    applicationcomment.created = DateTime.Now;
                    applicationcomment.created_by = User.Identity.Name;
                    applicationcomment.modified = DateTime.Now;
                    applicationcomment.modified_by = User.Identity.Name;
                    db.ApplicationComments.Add(applicationcomment);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return HttpNotFound("Failed to add Application Comment.<br/><br/>" + e.Message);
                }
            }
            return Content(Url.Action("Index", "ApplicationComment", new { application_id = applicationcomment.application_id }));
        }

        //
        // GET: /ApplicationComment/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            ApplicationComment applicationcomment = db.ApplicationComments.Find(id);
            if (applicationcomment == null)
            {
                return HttpNotFound("Application Comment not found.");
            }
            return View(applicationcomment);
        }

        //
        // POST: /ApplicationComment/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationComment applicationcomment)
        {
            try
            {
                applicationcomment.modified = DateTime.Now;
                applicationcomment.modified_by = User.Identity.Name;
                db.Entry(applicationcomment).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return HttpNotFound("Failed to edit Application Comment.<br/><br/>" + e.Message);
            }
            return Content(Url.Action("Index", "ApplicationComment", new { application_id = applicationcomment.application_id }));

        }

        //
        // GET: /ApplicationComment/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0)
        {
            ApplicationComment applicationcomment = db.ApplicationComments.Find(id);
            if (applicationcomment == null)
            {
                return HttpNotFound("Application Comment not found.");
            }
            var application_id = applicationcomment.application_id;
            try
            {
                db.ApplicationComments.Remove(applicationcomment);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return HttpNotFound("Failed to delete Application Comment.<br/><br/>" + e.Message);
            }
            return RedirectToAction("Index", "ApplicationComment", new { application_id = application_id });
        }

        //
        // POST: /ApplicationComment/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    ApplicationComment applicationcomment = db.ApplicationComments.Find(id);
        //    db.ApplicationComments.Remove(applicationcomment);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //
        // GET: /ApplicationComment/Import/
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Import()
        {
            //clear files uploaded but not used
            if (Directory.Exists(Server.MapPath("~/App_Data/Import/ApplicationComment/" + User.Identity.Name)))
            {
                var files = Directory.GetFiles(Server.MapPath("~/App_Data/Import/ApplicationComment/" + User.Identity.Name));
                foreach (var file in files)
                {
                    System.IO.File.Delete(file);
                }
            }
            return View();
        }

        //
        // POST: /ApplicationComment/Import/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Import(string filename)
        {
            ViewBag.filename = filename;
            var filepath = Server.MapPath("~/App_Data/Import/ApplicationComment/" + User.Identity.Name + "/" + filename);
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
                var comments = from c in excel.Worksheet<ApplicationComment>(sheetnames.First())
                               select c;
                foreach (var applicationcomment in comments)
                {
                    if (!String.IsNullOrEmpty(applicationcomment.comment))
                    {
                        applicationcomment.created = DateTime.Now;
                        applicationcomment.created_by = User.Identity.Name;
                        applicationcomment.modified = DateTime.Now;
                        applicationcomment.modified_by = User.Identity.Name;
                        db.ApplicationComments.Add(applicationcomment);
                        count++;
                    }
                }
                db.SaveChanges();
                Session["FlashMessage"] = count + " record(s) successfully imported.";
                //clear files uploaded after import
                if (Directory.Exists(Server.MapPath("~/App_Data/Import/ApplicationComment/" + User.Identity.Name)))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Import/ApplicationComment/" + User.Identity.Name));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to import application comments from excel file. <br/><br/>" + HttpUtility.JavaScriptStringEncode(e.Message);
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