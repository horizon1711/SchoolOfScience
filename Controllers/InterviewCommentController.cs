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
    public class InterviewCommentController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /InterviewComment/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index(int interview_id = 0, string student_id = null)
        {
            var interviewcomments = db.InterviewComments.Where(c => c.interview_id == interview_id && (student_id == null || c.Application.student_id == student_id));
            return View(interviewcomments.ToList());
        }

        //
        // GET: /InterviewComment/Details/5

        //public ActionResult Details(int id = 0)
        //{
        //    InterviewComment interviewcomment = db.InterviewComments.Find(id);
        //    if (interviewcomment == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(interviewcomment);
        //}

        //
        // GET: /InterviewComment/Create/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create(int interview_id = 0, int application_id = 0)
        {
            var interview = db.Interviews.SingleOrDefault(i => i.id == interview_id);
            if (interview == null)
            {
                return HttpNotFound("Interview Session not found.");
            }
            var application = interview.Applications.SingleOrDefault(a => a.id == application_id);
            if (application == null)
            {
                return HttpNotFound("Application not found.");
            }
            InterviewComment interviewcomment = new InterviewComment
            {
                interview_id = interview.id,
                Interview = interview,
                application_id = application_id,
                Application = application
            };

            return View(interviewcomment);
        }

        //
        // POST: /InterviewComment/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(InterviewComment interviewcomment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    interviewcomment.created = DateTime.Now;
                    interviewcomment.created_by = User.Identity.Name;
                    interviewcomment.modified = DateTime.Now;
                    interviewcomment.modified_by = User.Identity.Name;
                    db.InterviewComments.Add(interviewcomment);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    return HttpNotFound("Failed to add Interview Comment.<br/><br/>" + e.Message);
                }
            }
            return RedirectToAction("Index", "InterviewComment", new { interview_id = interviewcomment.interview_id });
        }

        //
        // GET: /InterviewComment/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            InterviewComment interviewcomment = db.InterviewComments.Find(id);
            if (interviewcomment == null)
            {
                return HttpNotFound("Interview Comment not found.");
            }
            return View(interviewcomment);
        }

        //
        // POST: /InterviewComment/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(InterviewComment interviewcomment)
        {
            try
            {
                interviewcomment.modified = DateTime.Now;
                interviewcomment.modified_by = User.Identity.Name;
                db.Entry(interviewcomment).State = EntityState.Modified;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return HttpNotFound("Failed to edit Interview Comment.<br/><br/>" + e.Message);
            }
            return RedirectToAction("Index", "InterviewComment", new { interview_id = interviewcomment.interview_id });

        }

        //
        // GET: /InterviewComment/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0)
        {
            InterviewComment interviewcomment = db.InterviewComments.Find(id);
            var interview_id = interviewcomment.interview_id;
            if (interviewcomment == null)
            {
                return HttpNotFound("Interview Comment not found.");
            }
            try
            {
                db.InterviewComments.Remove(interviewcomment);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                return HttpNotFound("Failed to delete Interview Comment.<br/><br/>" + e.Message);
            }
            return RedirectToAction("Index", "InterviewComment", new { interview_id = interview_id });
        }

        //
        // POST: /InterviewComment/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    InterviewComment interviewcomment = db.InterviewComments.Find(id);
        //    db.InterviewComments.Remove(interviewcomment);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

        //
        // GET: /StudentActivity/Import/
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Import()
        {
            //clear files uploaded but not used
            if (Directory.Exists(Server.MapPath("~/App_Data/Import/InterviewComment/" + User.Identity.Name)))
            {
                var files = Directory.GetFiles(Server.MapPath("~/App_Data/Import/InterviewComment/" + User.Identity.Name));
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
            var filepath = Server.MapPath("~/App_Data/Import/InterviewComment/" + User.Identity.Name + "/" + filename);
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
                var comments = from c in excel.Worksheet<InterviewComment>(sheetnames.First())
                                 select c;
                foreach (var interviewcomment in comments)
                {
                    if (!String.IsNullOrEmpty(interviewcomment.comment))
                    {
                        interviewcomment.created = DateTime.Now;
                        interviewcomment.created_by = User.Identity.Name;
                        interviewcomment.modified = DateTime.Now;
                        interviewcomment.modified_by = User.Identity.Name;
                        db.InterviewComments.Add(interviewcomment);
                        count++;
                    }
                }
                db.SaveChanges();
                Session["FlashMessage"] = count + " record(s) successfully imported.";
                //clear files uploaded after import
                if (Directory.Exists(Server.MapPath("~/App_Data/Import/InterviewComment/" + User.Identity.Name)))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Import/InterviewComment/" + User.Identity.Name));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to import interview comments from excel file. <br/><br/>" + HttpUtility.JavaScriptStringEncode(e.Message);
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