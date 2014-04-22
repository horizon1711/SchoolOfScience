using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using WebMatrix.WebData;
using SchoolOfScience.Models.ViewModels;

namespace SchoolOfScience.Controllers
{
    public class StudentProfileController : ControllerBase
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /StudentProfile/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor,Nominator")]
        public ActionResult Index()
        {
            ViewBag.careerList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_career }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.groupList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_group }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.departmentList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_organization }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.planList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_plan_description }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.levelList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_level }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.showTable = false;
            return View(new List<StudentProfile>().ToList());
        }

        //
        // POST: /StudentProfile/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor,ProgramAdmin,Nominator")]
        public ActionResult Index(FormCollection Form, bool assigned, bool requiredinterview, bool withcomment)
        {
            var students = db.StudentProfiles;
            ViewBag.studentid = Form["studentid"];
            ViewBag.firstname = Form["firstname"];
            ViewBag.lastname = Form["lastname"];
            ViewBag.careerList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_career }).Distinct().OrderBy(t => t.text), "text", "text", Form["career"]);
            ViewBag.groupList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_group }).Distinct().OrderBy(t => t.text), "text", "text", Form["academic_group"]);
            ViewBag.departmentList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_organization }).Distinct().OrderBy(t => t.text), "text", "text", Form["academic_organization"]);
            ViewBag.planList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_plan_description }).Distinct().OrderBy(t => t.text), "text", "text", Form["academic_plan_description"]);
            ViewBag.levelList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_level }).Distinct().OrderBy(t => t.text), "text", "text", Form["academic_level"]);
            ViewBag.commentkeyword = Form["commentkeyword"];
            ViewBag.showTable = true;
            return View(students.ToList().Where(s => (true)
                && (String.IsNullOrEmpty(Form["career"]) || s.academic_career == Form["career"])
                && (String.IsNullOrEmpty(Form["academic_group"]) || s.academic_group == Form["academic_group"])
                && (String.IsNullOrEmpty(Form["academic_organization"]) || s.academic_organization == Form["academic_organization"])
                && (String.IsNullOrEmpty(Form["academic_plan_description"]) || s.academic_plan_description == Form["academic_plan_description"])
                && (String.IsNullOrEmpty(Form["academic_level"]) || s.academic_level == Form["academic_level"])
                && (String.IsNullOrEmpty(Form["studentid"]) || s.id == Form["studentid"])
                && (String.IsNullOrEmpty(Form["firstname"]) || (s.name.IndexOf(Form["firstname"], StringComparison.OrdinalIgnoreCase) >= 0 && s.name.IndexOf(Form["firstname"], StringComparison.OrdinalIgnoreCase) > s.name.IndexOf(",")))
                && (String.IsNullOrEmpty(Form["lastname"]) || (s.name.IndexOf(Form["lastname"], StringComparison.OrdinalIgnoreCase) >= 0 && s.name.IndexOf(Form["lastname"], StringComparison.OrdinalIgnoreCase) < s.name.IndexOf(",")))
                && (String.IsNullOrEmpty(Form["commentkeyword"]) || s.StudentAdvisingRemarks.Any(r => 
                    r.text.IndexOf(Form["commentkeyword"], StringComparison.OrdinalIgnoreCase) >= 0
                    && (!r.@private || r.created_by == User.Identity.Name)))
                && (!assigned || s.Applications.Any(a => a.Interviews.Count() > 0))
                && (!requiredinterview || s.Applications.Any(a => a.Program.require_interview && a.Interviews.Count() == 0))
                && (!withcomment || s.StudentAdvisingRemarks.Count() > 0)
                ));
        }

        //
        // GET: /StudentProfile/MyStudent

        [Authorize(Roles = "FacultyAdvisor")]
        public ActionResult MyStudent(bool advisor = false, bool mentor = false)
        {
            ViewBag.careerList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_career }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.groupList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_group }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.departmentList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_organization }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.planList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_plan_description }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.levelList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_level }).Distinct().OrderBy(t => t.text), "text", "text");
            ViewBag.showTable = true;
            var students = db.StudentProfiles.Where(s => s.StudentAdvisors.Any(a => a.advisor_email.Substring(0, a.advisor_email.IndexOf("@")) == User.Identity.Name)
                    && (!(advisor && mentor) || (s.academic_plan_description.Contains("4Y")))
                    && (!(advisor && !mentor) || (s.academic_plan_description.Contains("4Y") && s.academic_plan_description.Contains("Undeclared")))
                    && (!(!advisor && mentor) || (s.academic_plan_description.Contains("4Y") && !s.academic_plan_description.Contains("Undeclared")))
                );
            return View(students.ToList());
        }

        //
        // POST: /StudentProfile/MyStudent

        [HttpPost]
        [Authorize(Roles = "FacultyAdvisor")]
        public ActionResult MyStudent(FormCollection Form, bool advisor = false, bool mentor = false)
        {
            var students = db.StudentProfiles.Where(s => s.StudentAdvisors.Any(a => a.advisor_email.Substring(0, a.advisor_email.IndexOf("@")) == User.Identity.Name));
            ViewBag.studentid = Form["studentid"];
            ViewBag.firstname = Form["firstname"];
            ViewBag.lastname = Form["lastname"];
            ViewBag.careerList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_career }).Distinct().OrderBy(t => t.text), "text", "text", Form["career"]);
            ViewBag.groupList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_group }).Distinct().OrderBy(t => t.text), "text", "text", Form["academic_group"]);
            ViewBag.departmentList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_organization }).Distinct().OrderBy(t => t.text), "text", "text", Form["academic_organization"]);
            ViewBag.planList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_plan_description }).Distinct().OrderBy(t => t.text), "text", "text", Form["academic_plan_description"]);
            ViewBag.levelList = new SelectList(db.StudentProfiles.Select(p => new { text = p.academic_level }).Distinct().OrderBy(t => t.text), "text", "text", Form["academic_level"]);
            ViewBag.commentkeyword = Form["commentkeyword"];
            ViewBag.showTable = true;
            return View(students.ToList().Where(s => (true)
                && (String.IsNullOrEmpty(Form["career"]) || s.academic_career == Form["career"])
                && (String.IsNullOrEmpty(Form["academic_group"]) || s.academic_group == Form["academic_group"])
                && (String.IsNullOrEmpty(Form["academic_organization"]) || s.academic_organization == Form["academic_organization"])
                && (String.IsNullOrEmpty(Form["academic_plan_description"]) || s.academic_plan_description == Form["academic_plan_description"])
                && (String.IsNullOrEmpty(Form["academic_level"]) || s.academic_level == Form["academic_level"])
                && (String.IsNullOrEmpty(Form["studentid"]) || s.id == Form["studentid"])
                && (String.IsNullOrEmpty(Form["firstname"]) || (s.name.IndexOf(Form["firstname"], StringComparison.OrdinalIgnoreCase) >= 0 && s.name.IndexOf(Form["firstname"], StringComparison.OrdinalIgnoreCase) < s.name.IndexOf(",")))
                && (String.IsNullOrEmpty(Form["lastname"]) || (s.name.IndexOf(Form["lastname"], StringComparison.OrdinalIgnoreCase) >= 0 && s.name.IndexOf(Form["lastname"], StringComparison.OrdinalIgnoreCase) > s.name.IndexOf(",")))
                && (String.IsNullOrEmpty(Form["commentkeyword"]) || s.StudentAdvisingRemarks.Any(r =>
                    r.text.IndexOf(Form["commentkeyword"], StringComparison.OrdinalIgnoreCase) >= 0
                    && (!r.@private && r.created_by == User.Identity.Name)))
                && (!(advisor && mentor) || (s.academic_plan_description.Contains("4Y")))
                && (!(advisor && !mentor) || (s.academic_plan_description.Contains("4Y") && s.academic_plan_description.Contains("Undeclared")))
                && (!(!advisor && mentor) || (s.academic_plan_description.Contains("4Y") && !s.academic_plan_description.Contains("Undeclared")))
                ));
        }

        //
        // GET: /StudentProfile/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,StudentDevelopment,FacultyAdvisor,EDP,CommTutor,ProgramAdmin,Nominator")]
        public ActionResult Details(string id = null)
        {
            StudentProfile studentprofile = db.StudentProfiles.Find(id);
            if (studentprofile == null)
            {
                Session["FlashMessage"] = "Student Profile not found";
                return RedirectToAction("Index", "StudentProfile");
            }
            StudentProfileViewModel ViewModel = new StudentProfileViewModel();
            ViewModel.student = studentprofile;
            var applications = studentprofile.Applications;
            List<ProgramAction> programactions = new List<ProgramAction>();
            foreach (var application in applications)
            {
                ProgramAction programaction = new ProgramAction();
                programaction.application = application;
                programaction.program = application.Program;
                programaction.student = studentprofile;

                //check student eligibility
                programaction.eligible = true;
                //check program application period
                programaction.inperiod = (application.Program.application_start_time <= DateTime.Now && DateTime.Now <= application.Program.application_end_time);
                //check if before program start time
                programaction.beforestart = (DateTime.Now < application.Program.application_start_time);
                //check existing application
                programaction.existed = true;
                //check application status
                programaction.saved = programaction.application.ApplicationStatus.editable;
                //check program status
                programaction.open = application.Program.ProgramStatus.open_for_application;
                programactions.Add(programaction);
            }
            ViewModel.programactions = programactions;
            ViewModel.particulartypes = db.StudentParticularTypes.ToList();
            ViewModel.experiencetypes = db.StudentExperienceTypes.ToList();
            ViewBag.profilepic = System.IO.File.Exists(Server.MapPath("~/Images/StudentProfile/" + studentprofile.id + ".jpg"));
            return PartialView(ViewModel);
        }

        //
        // GET: /StudentProfile/MyProfile/

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult MyProfile()
        {
            StudentProfileViewModel ViewModel = new StudentProfileViewModel();

            string studentid = User.Identity.Name;
            var studentprofile = db.StudentProfiles.Find(studentid);
            if (studentprofile == null)
            {
                Session["FlashMessage"] = "Student Profile not found";
                return RedirectToAction("Index", "Home");
            }
            var applications = db.Applications.ToList().Where(a => a.created_by == studentid.ToString());

            ViewModel.student = studentprofile;
            List<ProgramAction> programactions = new List<ProgramAction>();
            foreach (var application in applications)
            {
                ProgramAction programaction = new ProgramAction();
                programaction.application = application;
                programaction.program = application.Program;
                programaction.student = studentprofile;

                //check student eligibility
                programaction.eligible = true;
                //check program application period
                programaction.inperiod = (application.Program.application_start_time <= DateTime.Now && DateTime.Now <= application.Program.application_end_time);
                //check if before program start time
                programaction.beforestart = (DateTime.Now < application.Program.application_start_time);
                //check existing application
                programaction.existed = true;
                //check application status
                programaction.saved = programaction.application.ApplicationStatus.editable;
                //check program status
                programaction.open = application.Program.ProgramStatus.open_for_application;
                programactions.Add(programaction);
            }
            ViewModel.programactions = programactions;
            return View(ViewModel);
        }

        //
        // GET: /StudentProfile/MyParticular/

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult MyParticular(string student_id = null)
        {
            StudentParticularViewModel ViewModel = new StudentParticularViewModel();
            ViewModel.types = db.StudentParticularTypes;
            ViewModel.student_id = student_id;
            ViewModel.particulars = db.StudentParticulars.Where(p => p.student_id == student_id);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return View(ViewModel);
        }

        //
        // GET: /StudentProfile/MyExperience/

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult MyExperience(string student_id = null)
        {
            StudentExperienceViewModel ViewModel = new StudentExperienceViewModel();
            ViewModel.types = db.StudentExperienceTypes;
            ViewModel.student_id = student_id;
            ViewModel.experiences = db.StudentExperiences.Where(p => p.student_id == student_id);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return View(ViewModel);
        }

        //
        // GET: /StudentProfile/MyQualification/

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult MyQualification(string student_id = null)
        {
            ViewBag.student_id = student_id;
            var qualifications = db.StudentQualifications.Where(p => p.student_id == student_id);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return View(qualifications.ToList());
        }

        //
        // GET: /StudentProfile/AdvisingRemark/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,CommTutor")]
        public ActionResult AdvisingRemark(string student_id = null, string opener_id = null)
        {
            ViewBag.student_id = student_id;
            ViewBag.opener_id = opener_id;
            if (opener_id == null)
            {
                Session["FlashMessage"] = "Opener ID undefined.";
            }
            var remarks = db.StudentAdvisingRemarks.Where(p => p.student_id == student_id);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return View(remarks.ToList());
        }

        //
        // GET: /StudentProfile/ActivityRecord/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,EDP,CommTutor,Nominator,ProgramAdmin")]
        public ActionResult ActivityRecord(string student_id = null, string opener_id = null)
        {
            ViewBag.student_id = student_id;
            ViewBag.opener_id = opener_id;
            if (opener_id == null)
            {
                Session["FlashMessage"] = "Opener ID undefined.";
            }
            var activities = db.StudentActivities.Where(p => p.student_id == student_id);
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            return View(activities.ToList());
        }

        //
        // GET: /StudentProfile/ExportWord/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult ExportWord(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                id = User.Identity.Name;
            }
            var studentprofile = db.StudentProfiles.Find(id);
            if (studentprofile == null)
            {
                Session["FlashMessage"] = "Student Profile not found";
                return RedirectToAction("Index", "Home");
            }

            string strHtml = RenderRazorViewToString("ExportWord", studentprofile);
            strHtml = HttpUtility.HtmlDecode(strHtml);//Html decoding
            byte[] b = System.Text.Encoding.UTF8.GetBytes(strHtml);//convert string to byte array
            Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            return File(b, "application/ms-word", studentprofile.name + " Profile Export " + String.Format("{0:yyyyMMddHHmm}", DateTime.Now) + ".doc");

            //Response.ClearContent();
            //Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            //Response.Buffer = true;
            //Response.AddHeader("content-disposition", "attachment; filename=MyExcelFile.xls");
            //Response.ContentType = "application/ms-excel";

            //Response.Charset = "";
            //StringWriter sw = new StringWriter();
            //HtmlTextWriter htw = new HtmlTextWriter(sw);

            //grid.RenderControl(htw);

            //Response.Output.Write(sw.ToString());
            //Response.Flush();
            //Response.End();

            //return View("MyView");
        }

        ////
        //// GET: /StudentProfile/Create

        //public ActionResult Create()
        //{
        //    return View();
        //}

        ////
        //// POST: /StudentProfile/Create

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(StudentProfile studentprofile)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.StudentProfiles.Add(studentprofile);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(studentprofile);
        //}

        ////
        //// GET: /StudentProfile/Edit/5

        //public ActionResult Edit(int id = 0)
        //{
        //    StudentProfile studentprofile = db.StudentProfiles.Find(id);
        //    if (studentprofile == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(studentprofile);
        //}

        ////
        //// POST: /StudentProfile/Edit/5

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(StudentProfile studentprofile)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.Entry(studentprofile).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(studentprofile);
        //}

        //
        // GET: /StudentProfile/EditRemarks/5

        public ActionResult EditRemarks(string id = null)
        {
            StudentProfile studentprofile = db.StudentProfiles.Find(id);
            if (studentprofile == null)
            {
                return HttpNotFound("Student Profile not found");
            }
            return View(studentprofile);
        }

        //
        // POST: /StudentProfile/EditRemarks/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRemarks(StudentProfile studentprofile)
        {
            StudentProfile student = db.StudentProfiles.Find(studentprofile.id);
            if (studentprofile == null)
            {
                return HttpNotFound("Student Profile not found");
            }
            student.remarks = studentprofile.remarks;
            db.SaveChanges();
            return Content(student.remarks);
        }

        ////
        //// GET: /StudentProfile/Delete/5

        //public ActionResult Delete(int id = 0)
        //{
        //    StudentProfile studentprofile = db.StudentProfiles.Find(id);
        //    if (studentprofile == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(studentprofile);
        //}

        ////
        //// POST: /StudentProfile/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    StudentProfile studentprofile = db.StudentProfiles.Find(id);
        //    db.StudentProfiles.Remove(studentprofile);
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