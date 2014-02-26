using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using System.IO;

namespace SchoolOfScience.Controllers
{
    public class StudentAdvisingRemarkController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /StudentAdvisingRemark/

        //public ActionResult Index()
        //{
        //    var studentadvisingremarks = db.StudentAdvisingRemarks.Include(s => s.StudentProfile);
        //    return View(studentadvisingremarks.ToList());
        //}

        //
        // GET: /StudentAdvisingRemark/Details/5

        //public ActionResult Details(int id = 0)
        //{
        //    StudentAdvisingRemark studentadvisingremark = db.StudentAdvisingRemarks.Find(id);
        //    if (studentadvisingremark == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(studentadvisingremark);
        //}

        //
        // GET: /StudentAdvisingRemark/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,CommTutor,Nominator")]
        public ActionResult Create(string student_id = null, string opener_id = null)
        {
            ViewBag.opener_id = opener_id;
            var student = db.StudentProfiles.Find(student_id);
            if (student == null)
            {
                return HttpNotFound("Student Profile not found.");
            }
            StudentAdvisingRemark studentadvisingremark = new StudentAdvisingRemark
            {
                display_date = DateTime.Now,
                @private = true,
                student_id = student.id,
                StudentProfile = student
            };
            return View(studentadvisingremark);
        }

        //
        // POST: /StudentAdvisingRemark/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,CommTutor,Nominator")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(StudentAdvisingRemark studentadvisingremark, string opener_id = null)
        {
            if (ModelState.IsValid)
            {
                db.StudentAdvisingRemarks.Add(studentadvisingremark);
                try
                {
                    studentadvisingremark.created = DateTime.Now;
                    studentadvisingremark.created_by = User.Identity.Name;
                    studentadvisingremark.modified = DateTime.Now;
                    studentadvisingremark.modified_by = User.Identity.Name;
                    db.SaveChanges();
                    if (!String.IsNullOrEmpty(studentadvisingremark.filename) && !String.IsNullOrEmpty(studentadvisingremark.filepath))
                    {
                        var sourcePath = Server.MapPath("~/App_Data/" + studentadvisingremark.filepath);
                        var sourceFilepath = Path.Combine(sourcePath, studentadvisingremark.filename);
                        var destPath = Server.MapPath("~/App_Data/" + "Attachments/AdvisingRemark/" + studentadvisingremark.id);
                        var destFilepath = Path.Combine(destPath, studentadvisingremark.filename);
                        try
                        {
                            Directory.CreateDirectory(destPath);
                        }
                        catch (Exception e)
                        {
                            Session["FlashMessage"] = "Failed to create directory." + e.Message;
                        }
                        try
                        {
                            System.IO.File.Move(sourceFilepath, destFilepath);
                            studentadvisingremark.filepath = "Attachments/AdvisingRemark/" + studentadvisingremark.id;
                            db.SaveChanges();
                        }
                        catch (Exception e)
                        {
                            Session["FlashMessage"] = "Failed to move file." + e.Message;
                        }
                    }

                    //clear temp files uploaded but not used
                    if (Directory.Exists(Server.MapPath("~/App_Data/Temp/AdvisingRemark/" + User.Identity.Name)))
                    {
                        var files = Directory.GetFiles(Server.MapPath("~/App_Data/Temp/AdvisingRemark/" + User.Identity.Name));
                        foreach (var file in files)
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                }
                catch (Exception e)
                {
                    return HttpNotFound("Failed to add remark.<br/><br/>" + e.Message);
                }
            }
            return RedirectToAction("AdvisingRemark", "StudentProfile", new { student_id = studentadvisingremark.student_id, opener_id = opener_id });
        }

        //
        // GET: /StudentAdvisingRemark/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,CommTutor,Nominator")]
        public ActionResult Edit(int id = 0, string opener_id = null)
        {
            ViewBag.opener_id = opener_id;
            StudentAdvisingRemark studentadvisingremark = db.StudentAdvisingRemarks.ToList().Where(p => p.id == id && (p.created_by == User.Identity.Name)).SingleOrDefault();
            if (studentadvisingremark == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            return View(studentadvisingremark);
        }

        //
        // POST: /StudentAdvisingRemark/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,CommTutor,Nominator")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(StudentAdvisingRemark studentadvisingremark, string opener_id = null)
        {
            if (ModelState.IsValid)
            {
                var remark = db.StudentAdvisingRemarks.Find(studentadvisingremark.id);
                if (remark.filename != studentadvisingremark.filename || remark.filepath != studentadvisingremark.filepath) // action only when existing filename is diff from posted one
                {
                    if (!String.IsNullOrEmpty(remark.filename)) //delete existing file if filename id not empty
                    {
                        var path = Server.MapPath("~/App_Data/" + remark.filepath);
                        var filepath = Path.Combine(path, remark.filename);
                        if (System.IO.File.Exists(filepath))
                        {
                            try
                            {
                                System.IO.File.Delete(filepath);
                            }
                            catch (Exception e)
                            {
                                Session["FlashMessage"] = "Failed to delete attachment." + e.Message;
                            }
                        }
                    }
                    if (!String.IsNullOrEmpty(studentadvisingremark.filename) && !String.IsNullOrEmpty(studentadvisingremark.filepath)) // move the uploaded file to destination
                    {
                        var sourcePath = Server.MapPath("~/App_Data/" + studentadvisingremark.filepath);
                        var sourceFilepath = Path.Combine(sourcePath, studentadvisingremark.filename);
                        var destPath = Server.MapPath("~/App_Data/" + "Attachments/AdvisingRemark/" + studentadvisingremark.id);
                        var destFilepath = Path.Combine(destPath, studentadvisingremark.filename);
                        try
                        {
                            Directory.CreateDirectory(destPath);
                        }
                        catch (Exception e)
                        {
                            Session["FlashMessage"] = "Failed to create directory." + e.Message;
                        }
                        try
                        {
                            System.IO.File.Move(sourceFilepath, destFilepath);
                            studentadvisingremark.filepath = "Attachments/AdvisingRemark/" + studentadvisingremark.id;
                        }
                        catch (Exception e)
                        {
                            Session["FlashMessage"] = "Failed to move file." + e.Message;
                        }
                    }

                    //clear temp files uploaded but not used
                    if (Directory.Exists(Server.MapPath("~/App_Data/Temp/AdvisingRemark/" + studentadvisingremark.id)))
                    {
                        var files = Directory.GetFiles(Server.MapPath("~/App_Data/Temp/AdvisingRemark/" + studentadvisingremark.id));
                        foreach (var file in files)
                        {
                            System.IO.File.Delete(file);
                        }
                    }
                }
                studentadvisingremark.modified = DateTime.Now;
                studentadvisingremark.modified_by = User.Identity.Name;
                db.Entry(remark).CurrentValues.SetValues(studentadvisingremark);
                db.SaveChanges();
            }
            return RedirectToAction("AdvisingRemark", "StudentProfile", new { student_id = studentadvisingremark.student_id, opener_id = opener_id });
        }

        //
        // GET: /StudentAdvisingRemark/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,CommTutor,Nominator")]
        public ActionResult Delete(int id = 0, string opener_id = null)
        {
            StudentAdvisingRemark studentadvisingremark = db.StudentAdvisingRemarks.ToList().Where(p => p.id == id && (p.created_by == User.Identity.Name)).SingleOrDefault();
            var student_id = studentadvisingremark.student_id;
            if (studentadvisingremark == null)
            {
                return HttpNotFound("The record you selected does not exist. Please refresh the page.");
            }
            db.StudentAdvisingRemarks.Remove(studentadvisingremark);
            db.SaveChanges();
            return RedirectToAction("AdvisingRemark", "StudentProfile", new { student_id = student_id, opener_id = opener_id });
        }

        //
        // POST: /StudentAdvisingRemark/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    StudentAdvisingRemark studentadvisingremark = db.StudentAdvisingRemarks.Find(id);
        //    db.StudentAdvisingRemarks.Remove(studentadvisingremark);
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