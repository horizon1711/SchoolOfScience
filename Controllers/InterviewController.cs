using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using SchoolOfScience.Models.ViewModels;
using SchoolOfScience.Attributes;
using WebMatrix.WebData;
using System.Data.Objects.SqlClient;

namespace SchoolOfScience.Controllers
{
    public class InterviewController : ControllerBase
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /Interview/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index()
        {
            var interview = db.Interviews;
            ViewBag.programList = new SelectList(db.Programs.Where(p => p.Interviews.Count() > 0).OrderBy(p => p.name), "id", "name");
            ViewBag.programTypeList = new SelectList(db.ProgramTypes, "id", "name");
            ViewBag.programStatusList = new SelectList(db.ProgramStatus, "id", "name");
            ViewBag.interviewVenueList = new SelectList(db.InterviewVenues, "id", "name");
            ViewBag.interviewStatusList = new SelectList(db.InterviewStatus, "id", "name");
            return View(interview.ToList());
        }

        //
        // POST: /Interview/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index(FormCollection Form, bool available, bool assigned, bool upcoming = false)
        {
            var interview = db.Interviews.Include(i => i.InterviewStatus).Include(i => i.Program).Include(i => i.Applications);
            ViewBag.programList = new SelectList(db.Programs.Where(p => p.Interviews.Count() > 0).OrderBy(p => p.name), "id", "name", Form["program"]);
            ViewBag.programTypeList = new SelectList(db.ProgramTypes, "id", "name", Form["program_type"]);
            ViewBag.programStatusList = new SelectList(db.ProgramStatus, "id", "name", Form["program_status"]);
            ViewBag.interviewVenueList = new SelectList(db.InterviewVenues, "id", "name", Form["interview_venue"]);
            ViewBag.interviewStatusList = new SelectList(db.InterviewStatus, "id", "name", Form["interview_status"]);
            ViewBag.startdate = Form["startdate"];
            ViewBag.enddate = Form["enddate"];
            return View(interview.ToList().Where(i => (true)
                    && (String.IsNullOrEmpty(Form["program"]) || i.program_id.ToString() == Form["program"])
                    && (String.IsNullOrEmpty(Form["program_type"]) || i.Program.type_id.ToString() == Form["program_type"])
                    && (String.IsNullOrEmpty(Form["program_status"]) || i.Program.status_id.ToString() == Form["program_status"])
                    && (String.IsNullOrEmpty(Form["interview_venue"]) || i.venue_id.ToString() == Form["interview_venue"])
                    && (String.IsNullOrEmpty(Form["interview_status"]) || i.status_id.ToString() == Form["interview_status"])
                    && (!available || i.Applications.Count() < i.no_of_interviewee)
                    && (!assigned || i.Applications.Count() > 0)
                    && (!upcoming || i.start_time > DateTime.Now)
                    && (String.IsNullOrEmpty(Form["startdate"]) || i.start_time > Convert.ToDateTime(Form["startdate"]))
                    && (String.IsNullOrEmpty(Form["enddate"]) || i.start_time < Convert.ToDateTime(Form["enddate"]).AddDays(1))
                    ));
        }

        //
        // GET: /Interview/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Details(int id = 0)
        {
            Interview interview = db.Interviews.Find(id);
            if (interview == null)
            {
                return HttpNotFound();
            }
            return View(interview);
        }

        //
        // GET: /Interview/CreateTimeslotSession

        [Ajax(true)]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult CreateTimeslotSession(int index)
        {
            ViewBag.index = index;
            return PartialView();
        }

        //
        // GET: /Interview/CreateTimeslotSkippedDate

        [Ajax(true)]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult CreateTimeslotSkippedDate(int index)
        {
            ViewBag.index = index;
            return PartialView();
        }

        //
        // GET: /Interview/CreateMultiple

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult CreateMultiple()
        {
            InterviewCreateMultipleViewModel ViewModel = new InterviewCreateMultipleViewModel();
            ViewModel.interview = new Interview();
            ViewBag.programList = new SelectList(db.Programs.Where(p => p.require_interview), "id", "name");
            ViewBag.venueList = new SelectList(db.InterviewVenues.Where(v => v.status), "id", "name");
            return View(ViewModel);
        }

        //
        // POST: /Interview/CreateMultiple

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMultiple(InterviewCreateMultipleViewModel ViewModel)
        {
            Interview interview = ViewModel.interview;
            var openStatus = db.InterviewStatus.FirstOrDefault(s => s.default_status);
            if (openStatus == null)
            {
                Session["FlashMessage"] = "Default Status not found.";
                return RedirectToAction("CreateMultiple");
            }
            int openedStatusId = openStatus.id;
            List<bool> availableDaysOfWeek = PrepareAvailableDaysOfWeek(ViewModel.config);
            List<Timeslot> timeslots = new List<Timeslot>();
            DateTime datecursor = ViewModel.config.start_date.Date;

            //initialize skipped_dates to avoid null reference 
            var skipped_dates = new List<DateTime>();
            if (ViewModel.skipped_dates != null)
            {
                skipped_dates = ViewModel.skipped_dates.ToList();
            }

            try
            {
                while (datecursor <= ViewModel.config.end_date.Date)
                {
                    if (availableDaysOfWeek[Convert.ToInt16(datecursor.DayOfWeek)] && !skipped_dates.Any(d => d.Date == datecursor.Date))
                    {
                        foreach (var session in ViewModel.sessions.Where(s => !s.excluded).ToList())
                        {
                            TimeSpan starttime = session.start_time.TimeOfDay;
                            TimeSpan duration = new TimeSpan(0, ViewModel.interview.duration + ViewModel.interview.buffer, 0);
                            TimeSpan endtime = starttime.Add(duration);

                            while (endtime <= session.end_time.TimeOfDay)
                            {
                                bool overlapped = false;
                                TimeSpan newstarttime = new TimeSpan();

                                foreach (var ex_session in ViewModel.sessions.Where(x => x.excluded))
                                {
                                    TimeSpan ex_starttime = ex_session.start_time.TimeOfDay;
                                    TimeSpan ex_endtime = ex_session.end_time.TimeOfDay;
                                    if ((starttime >= ex_starttime && starttime < ex_endtime)
                                        || (endtime > ex_starttime && endtime <= ex_endtime)
                                        || (starttime <= ex_starttime && endtime >= ex_endtime))
                                    {
                                        overlapped = true;
                                        newstarttime = ex_endtime > newstarttime ? ex_endtime : newstarttime;
                                        goto Action;
                                    }
                                }

                                foreach (var ex_session in db.Interviews.Where(i => i.venue_id == interview.venue_id))
                                {
                                    DateTime ex_starttime = ex_session.start_time;
                                    DateTime ex_endtime = ex_session.end_time;
                                    if ((datecursor.Add(starttime) >= ex_starttime && datecursor.Add(starttime) < ex_endtime)
                                        || (datecursor.Add(endtime) > ex_starttime && datecursor.Add(endtime) <= ex_endtime)
                                        || (datecursor.Add(starttime) <= ex_starttime && datecursor.Add(endtime) >= ex_endtime))
                                    {
                                        overlapped = true;
                                        newstarttime = ex_endtime.TimeOfDay > newstarttime ? ex_endtime.TimeOfDay : newstarttime;
                                        goto Action;
                                    }
                                }

                            Action:
                                if (!overlapped && endtime <= session.end_time.TimeOfDay)
                                {
                                    interview.start_time = datecursor.Add(starttime);
                                    interview.end_time = datecursor.Add(endtime);
                                    interview.status_id = openedStatusId;
                                    db.Interviews.Add(interview);
                                    db.SaveChanges();
                                    starttime = endtime;
                                }
                                else
                                {
                                    starttime = newstarttime;
                                }
                                endtime = starttime.Add(duration);
                            }
                        }
                    }
                    datecursor = datecursor.AddDays(1);
                }
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.programList = new SelectList(db.Programs.Where(p => p.require_interview), "id", "name");
                ViewBag.venueList = new SelectList(db.InterviewVenues.Where(v => v.status), "id", "name");
                Session["FlashMessage"] = "Failed to create interview timeslots." + e.Message; 
                return View(ViewModel);
            }
        }

        //
        // GET: /Interview/AssignAvoidedSession

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult AssignAvoidedSession(int index)
        {
            InterviewAssignMultipleViewModel ViewModel = new InterviewAssignMultipleViewModel();
            ViewBag.index = index;
            ViewBag.academicOrganizationList = new SelectList(db.StudentProfiles.Select(s => new { value = s.academic_organization, text = s.academic_organization }).Distinct(), "value", "text");
            ViewBag.academicPlanList = new SelectList(db.StudentProfiles.Select(s => new { value = s.academic_plan_primary, text = s.academic_plan_description }).Distinct(), "value", "text");
            ViewBag.academicLevelList = new SelectList(db.StudentProfiles.Select(s => new { value = s.academic_level, text = s.academic_level }).Distinct(), "value", "text");
            return PartialView(ViewModel);
        }

        //
        // GET: /Interview/AssignMultiple

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult AssignMultiple()
        {
            InterviewAssignMultipleViewModel ViewModel = new InterviewAssignMultipleViewModel();
            ViewModel.sort_by_dept = true;
            ViewModel.continuous_assign = true;
            ViewBag.programList = new SelectList(db.Programs.Where(p => p.require_interview && p.Applications.Count(a => a.ApplicationStatus.name == "Processed") > 0), "id", "name");
            ViewBag.academicOrganizationList = new SelectList(db.StudentProfiles.Select(s => new { value = s.academic_organization, text = s.academic_organization }).Distinct(), "value", "text");
            ViewBag.academicPlanList = new SelectList(db.StudentProfiles.Select(s => new { value = s.academic_plan_primary, text = s.academic_plan_description }).Distinct(), "value", "text");
            ViewBag.academicLevelList = new SelectList(db.StudentProfiles.Select(s => new { value = s.academic_level, text = s.academic_level }).Distinct(), "value", "text");
            return View(ViewModel);
        }

        //
        // POST: /Interview/AssignMultiple

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult AssignMultiple(InterviewAssignMultipleViewModel ViewModel)
        {
            int auto_assigned_count = 0;

            Program program = db.Programs.Find(ViewModel.program_id);
            if (program == null)
            {
                Session["FlashMessage"] = "Program not found.";
            }
            try
            {
                foreach (var interview in program.Interviews.Where(i => i.Applications.Count() < i.no_of_interviewee))
                {
                    string department = null;
                    var applications = program.Applications.Where(a => a.Interviews.Count() == 0 && a.ApplicationStatus.name == "Processed");
                    //sort applications by department is sort_by_dept is true, ,or continuous_assign is false
                    if (ViewModel.sort_by_dept || !ViewModel.continuous_assign)
                    {
                        applications = applications.OrderBy(a => a.StudentProfile.academic_organization);
                    }
                    foreach (var application in applications)
                    {
                        //check if student academic organization is set in avoided session and collide with interview session
                        bool avoided = CheckAvoidedSessions(interview, ViewModel.avoided_sessions, application.StudentProfile);
                        if (!avoided && interview.Applications.Count() < interview.no_of_interviewee)
                        {
                            //check if same department in a single interview if continuous_assign is false
                            if (ViewModel.continuous_assign || department == null || department == application.StudentProfile.academic_organization)
                            {
                                interview.Applications.Add(application);
                                application.Interviews.Add(interview);
                                auto_assigned_count++;
                                department = application.StudentProfile.academic_organization;
                            }
                        }
                    }
                }
                db.Entry(program).State = EntityState.Modified;
                db.SaveChanges();

                var available_interview_seat_count = program.Interviews.Sum(i => i.no_of_interviewee - i.Applications.Count());
                int unassigned_application_count = program.Applications.Where(a => a.ApplicationStatus.name == "Processed" && a.Interviews.Count() == 0).Count();

                Session["FlashMessage"] = "<b>Auto assign summary:</b><br/>"
                    + "Auto-assigned Applications : " + auto_assigned_count + "<br/>"
                    + "Available seats: " + available_interview_seat_count + "<br/>"
                    + "Unassigned Applications : " + unassigned_application_count;
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to assign applications to interview timeslots." + (e.Message);
                return RedirectToAction("AssignMultiple");
            }
        }

        protected Notification CreateNotification(Interview interview, Application application)
        {
            NotificationType type = db.NotificationTypes.Where(t => t.name == "InterviewAssigned").SingleOrDefault();
            NotificationStatus status = db.NotificationStatus.Where(s => s.name == "Pending").SingleOrDefault();
            string body = "";
            string subject = "";
            if (type.NotificationTemplate != null)
            {
                body = type.NotificationTemplate.body;
                subject = type.NotificationTemplate.subject;
            }
            Notification notification = new Notification{
                send_time = DateTime.Now.AddMinutes(30),
                status_id = status.id,
                type_id = type.id,
                body = body,
                subject = subject,
                sender = "test@test.com",
                interview_id = interview.id,
                created = DateTime.Now,
                created_by = User.Identity.Name,
                modified = DateTime.Now,
                modified_by = User.Identity.Name
            };
            return notification;
        }

        [Ajax(true)]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult AssignApplicationList(int id)
        {
            IEnumerable<Application> applications = db.Applications.Where(a => a.program_id == id && a.ApplicationStatus.name == "Processed" && a.Interviews.Count() == 0);
            return PartialView(applications.ToList());
        }

        protected bool CheckAvoidedSessions(Interview interview, IList<AvoidedSession> sessions, StudentProfile student)
        {
            if (sessions != null)
            {
                foreach (var session in sessions)
                {
                    if ((session.academic_organizations == null || session.academic_organizations.ToList().Contains(student.academic_organization)) &&
                        (session.academic_plans == null || session.academic_plans.ToList().Contains(student.academic_plan_primary)) &&
                        (session.academic_levels == null || session.academic_levels.ToList().Contains(student.academic_level)))
                    {
                        List<bool> avoidedDayOfWeekList = PrepareAvoidedDayOfWeek(session);
                        if (avoidedDayOfWeekList[Convert.ToInt16(interview.start_time.DayOfWeek)])
                        {
                            if ((interview.start_time.TimeOfDay >= session.start_time.TimeOfDay && interview.start_time.TimeOfDay < session.end_time.TimeOfDay) ||
                                (interview.end_time.TimeOfDay > session.start_time.TimeOfDay && interview.end_time.TimeOfDay <= session.end_time.TimeOfDay) ||
                                (interview.start_time.TimeOfDay <= session.start_time.TimeOfDay && interview.end_time.TimeOfDay >= session.end_time.TimeOfDay))
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        //
        // GET: /Interview/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create()
        {
            var openStatus = db.InterviewStatus.FirstOrDefault(s => s.default_status);
            if (openStatus == null)
            {
                Session["FlashMessage"] = "Default Status not found.";
                return RedirectToAction("Index");
            }
            int openedStatusId = openStatus.id;
            ViewBag.statusList = new SelectList(db.InterviewStatus, "id", "name", openedStatusId);
            ViewBag.programList = new SelectList(db.Programs.Where(p => p.require_interview), "id", "name");
            ViewBag.venueList = new SelectList(db.InterviewVenues.Where(v => v.status), "id", "name");
            return View();
        }

        //
        // POST: /Interview/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Interview interview)
        {
            if (ModelState.IsValid)
            {
                db.Interviews.Add(interview);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create interview timeslot." + e.Message;
                    return View(interview);
                }
                return RedirectToAction("Index");
            }

            ViewBag.statusList = new SelectList(db.InterviewStatus, "id", "name", interview.status_id);
            ViewBag.programList = new SelectList(db.Programs.Where(p => p.require_interview), "id", "name");
            ViewBag.venueList = new SelectList(db.InterviewVenues.Where(v => v.status), "id", "name");
            return View(interview);
        }

        //
        // GET: /Interview/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            InterviewViewModel ViewModel = new InterviewViewModel();
            Interview interview = db.Interviews.Find(id);
            if (interview == null)
            {
                return HttpNotFound();
            }
            ViewModel.interview = interview;
            ViewModel.applications = interview.Applications.ToList();
            ViewBag.statusList = new SelectList(db.InterviewStatus, "id", "name", interview.status_id);
            ViewBag.programList = new SelectList(db.Programs.Where(p => p.require_interview), "id", "name");
            var applicationIds = interview.Applications.Select(x => x.id);
            ViewBag.applicationList = new SelectList(db.Applications.Where(a => a.program_id == interview.program_id && (a.Interviews.Count() == 0 || applicationIds.Contains(a.id))), "id", "StudentProfile.name");
            ViewBag.venueList = new SelectList(db.InterviewVenues.Where(v => v.status), "id", "name");
            
            return View(ViewModel);
        }

        //
        // POST: /Interview/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(InterviewViewModel ViewModel)
        {
            Interview interview = db.Interviews.Find(ViewModel.interview.id);
            db.Entry(interview).CurrentValues.SetValues(ViewModel.interview);
            interview.Applications.Clear();
            if (ViewModel.applications != null)
            {
                foreach (var application in ViewModel.applications)
                {
                    interview.Applications.Add(db.Applications.Find(application.id));
                }
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to edit interview timeslot." + e.Message;
                ViewBag.statusList = new SelectList(db.InterviewStatus, "id", "name", interview.status_id);
                ViewBag.programList = new SelectList(db.Programs.Where(p => p.require_interview), "id", "name");
                ViewBag.venueList = new SelectList(db.InterviewVenues.Where(v => v.status), "id", "name");
                return View(ViewModel);
            }
            return RedirectToAction("Index");
        }

        public ActionResult AssignApplicationDropdown(int index, int id)
        {
            InterviewViewModel ViewModel = new InterviewViewModel();
            Interview interview = db.Interviews.Find(id);
            ViewModel.interview = interview;
            ViewBag.index = index;
            var applicationIds = interview.Applications.Where(a => a.ApplicationStatus.name == "Processed").Select(x => x.id);
            ViewBag.applicationList = new SelectList(db.Applications.Where(a => a.program_id == interview.program_id && (a.Interviews.Count() == 0 || applicationIds.Contains(a.id))), "id", "StudentProfile.name");
            return PartialView(ViewModel);
        }

        //
        // GET: /Interview/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0)
        {
            Interview interview = db.Interviews.Find(id);
            if (interview == null)
            {
                Session["FlashMessage"] = "Interview not found.";
                return RedirectToAction("Index");
            }
            if (interview.Applications != null && interview.Applications.Count() > 0)
            {
                Session["FlashMessage"] = "Interview is attached to existing Application(s).";
                return RedirectToAction("Index");
            }
            return View(interview);
        }

        //
        // POST: /Interview/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Interview interview = db.Interviews.Find(id);
            db.Interviews.Remove(interview);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete interview timeslot." + e.Message;
                return View(interview);
            }
            return RedirectToAction("Index");
        }

        //
        // GET: /Interview/BatchDelete/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult BatchDelete(string items)
        {
            var i = items.Split('_');
            var interviews = db.Interviews.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            ViewBag.items = items;
            return PartialView(interviews.ToList());
        }

        //
        // POST: /Interview/BatchDelete/

        [HttpPost, ActionName("BatchDelete")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult BatchDeleteConfirmed(string items)
        {
            var i = items.Split('_');
            var interviews = db.Interviews.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            foreach (var interview in interviews)
            {
                db.Interviews.Remove(interview);
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to update interviews. <br/><br/>" + e.Message;
            }
            return RedirectToAction("Index");
        }

        //
        // GET: /Interview/BatchUpdate/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult BatchUpdate(string items)
        {
            var i = items.Split('_');
            var interviews = db.Interviews.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            ViewBag.statusList = new SelectList(db.InterviewStatus, "id", "name");
            ViewBag.items = items;
            return PartialView(interviews.ToList());
        }

        //
        // POST: /Interview/BatchUpdate/

        [HttpPost, ActionName("BatchUpdate")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult BatchUpdateConfirmed(string items, int status_id)
        {
            var i = items.Split('_');
            var interviews = db.Interviews.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            var status = db.InterviewStatus.Find(status_id);
            foreach (var interview in interviews)
            {
                interview.status_id = status_id;
                if (status.name == "Notified")
                {
                    foreach (var application in interview.Applications)
                    {
                        SendNotification(CreateNotification("InterviewAssigned", interview, application));
                    }
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to update interviews. <br/><br/>" + e.Message;
            }
            return RedirectToAction("Index");
        }

        //
        // GET: /Interview/CommentTemplate/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult CommentTemplate(string items)
        {
            var i = items.Split('_');
            var interviews = db.Interviews.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));

            string strHtml = RenderRazorViewToString("CommentTemplate", interviews.ToList());
            strHtml = HttpUtility.HtmlDecode(strHtml);//Html解碼
            byte[] b = System.Text.Encoding.UTF8.GetBytes(strHtml);//字串轉byte陣列
            Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            return File(b, "application/vnd.ms-excel", "Comment Template " + String.Format("{0:yyyyMMddHHmm}", DateTime.Now) + ".xls");//輸出檔案給Client端
        }

        public List<bool> PrepareAvailableDaysOfWeek(TimeslotConfig config)
        {
            List<bool> list = new List<bool>();
            list.Add(config.sunday);
            list.Add(config.monday);
            list.Add(config.tuesday);
            list.Add(config.wednesday);
            list.Add(config.thursday);
            list.Add(config.friday);
            list.Add(config.saturday);
            return list;
        }

        public List<bool> PrepareAvoidedDayOfWeek(AvoidedSession session)
        {
            List<bool> list = new List<bool>();
            list.Add(session.sunday);
            list.Add(session.monday);
            list.Add(session.tuesday);
            list.Add(session.wednesday);
            list.Add(session.thursday);
            list.Add(session.friday);
            list.Add(session.saturday);
            return list;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}