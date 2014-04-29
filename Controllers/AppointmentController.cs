using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using SchoolOfScience.Attributes;
using SchoolOfScience.Models.ViewModels;
using System.Data.Objects.SqlClient;

namespace SchoolOfScience.Controllers
{
    public class AppointmentController : ControllerBase
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();
        private SchoolOfScienceEntities programdb = new SchoolOfScienceEntities();

        //
        // GET: /Appointment/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult Index(bool consultation = false, bool advisor = false, bool mentor = false)
        {
            IQueryable<Appointment> appointments;
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                appointments = db.Appointments;
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
                ViewBag.reserved = true;
                ViewBag.available = true;
            }
            else if (User.IsInRole("ProgramAdmin"))
            {
                appointments = db.Appointments.Where(a => a.AppointmentHost.UserProfiles1.Any(u => u.UserName == User.Identity.Name));
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.UserProfiles1.Any(u => u.UserName == User.Identity.Name)), "id", "name");
                consultation = false;
                ViewBag.reserved = true;
                ViewBag.available = true;
            }
            else
            {
                appointments = db.Appointments.Where(a => a.AppointmentHost.SystemUsers.Any(u => u.UserName == User.Identity.Name)
                    && (!(advisor && mentor) || (a.StudentProfile.academic_plan_description.Contains("4Y")))
                    && (!(advisor && !mentor) || (a.StudentProfile.academic_plan_description.Contains("4Y") && a.StudentProfile.academic_plan_description.Contains("Undeclared")) && !a.AppointmentConcerns.Any(c => c.program_id != null))
                    && (!(!advisor && mentor) || (a.StudentProfile.academic_plan_description.Contains("4Y") && !a.StudentProfile.academic_plan_description.Contains("Undeclared")) && !a.AppointmentConcerns.Any(c => c.program_id != null))
                    );
                ViewBag.reserved = true;
                ViewBag.available = true;
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name");
            ViewBag.consultation = consultation;
            ViewBag.host = db.AppointmentHosts.FirstOrDefault(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name));
            return View(appointments.ToList().Where(a => (true)
                && a.start_time > DateTime.Now
                && (!consultation || a.AppointmentConcerns.Any(c => c.program_id != null))
            ));
        }

        //
        // POST: /Appointment/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult Index(FormCollection Form, bool reserved, bool available, bool past, bool consultation = false, bool advisor = false, bool mentor = false)
        {
            IQueryable<Appointment> appointments;
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                appointments = db.Appointments;
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name", Form["host"]);
            }
            else if (User.IsInRole("ProgramAdmin"))
            {
                appointments = db.Appointments.Where(a => a.AppointmentHost.UserProfiles1.Any(u => u.UserName == User.Identity.Name));
                consultation = false;
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.UserProfiles1.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            else
            {
                appointments = db.Appointments.Where(a => a.AppointmentHost.SystemUsers.Any(u => u.UserName == User.Identity.Name));
                consultation = false;
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name", Form["host"]);
            }
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name", Form["concern"]);
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", Form["status"]);
            ViewBag.consultation = consultation;
            ViewBag.startdate = Form["startdate"];
            ViewBag.enddate = Form["enddate"];
            ViewBag.host = db.AppointmentHosts.FirstOrDefault(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name));
            return View(appointments.ToList().Where(a => (true)
                && (String.IsNullOrEmpty(Form["concern"]) || a.AppointmentConcerns.Any(c => c.id.ToString() == Form["concern"]))
                && (String.IsNullOrEmpty(Form["host"]) || a.host_id.ToString() == Form["host"])
                && (String.IsNullOrEmpty(Form["status"]) || a.status_id.ToString() == Form["status"])
                && (!(reserved && !available) || a.student_id != null)
                && (!(!reserved && available) || a.student_id == null)
                && (past || a.start_time > DateTime.Now)
                && (!consultation || a.AppointmentConcerns.Any(c => c.program_id != null))
                && (!(advisor && mentor) || (a.student_id != null && a.StudentProfile.academic_plan_description.Contains("4Y")))
                && (!(advisor && !mentor) || (a.student_id != null && a.StudentProfile.academic_plan_description.Contains("4Y") && a.StudentProfile.academic_plan_description.Contains("Undeclared")) && !a.AppointmentConcerns.Any(c => c.program_id != null))
                && (!(!advisor && mentor) || (a.student_id != null && a.StudentProfile.academic_plan_description.Contains("4Y") && !a.StudentProfile.academic_plan_description.Contains("Undeclared")) && !a.AppointmentConcerns.Any(c => c.program_id != null))
                && (String.IsNullOrEmpty(Form["startdate"]) || a.start_time > Convert.ToDateTime(Form["startdate"]))
                && (String.IsNullOrEmpty(Form["enddate"]) || a.start_time < Convert.ToDateTime(Form["enddate"]).AddDays(1))
                ));
        }

        //
        // GET: /Appointment/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult Details(int id = 0)
        {
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                Session["FlashMessage"] = "Appointment not found.";
                return RedirectToAction("Index");
            }
            return View(appointment);
        }

        //
        // GET: /Appointment/Calendar

        public ActionResult Calendar(int programid = 0)
        {
            var appointments = db.Appointments.Where(a => a.AppointmentConcerns.Any(c => c.program_id == programid));
            return View(appointments.ToList());
        }

        //
        // GET: /Appointment/BookingCalendar

        public ActionResult BookingCalendar(int host_id = 0)
        {
            DateTime dt24 = DateTime.Now.AddHours(24);
            var appointments = db.Appointments.Where(o => o.host_id == host_id && o.start_time > dt24);
            return View(appointments.ToList());
        }

        //
        // GET: /Appointment/HostCalendar

        public ActionResult HostCalendar(int host_id = 0)
        {
            var appointments = db.Appointments.Where(o => o.host_id == host_id);
            ViewBag.hostname = db.AppointmentHosts.Find(host_id).name;
            return View(appointments.ToList());
        }

        //
        // GET: /Appointment/HostCalendarDetails

        [Ajax(true)]
        public ActionResult HostCalendarDetails(int id = 0)
        {
            var appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound("Appointment not found.");
            }
            return View(appointment);
        }

        //
        // GET: /Appointment/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult Create()
        {
            var status = db.AppointmentStatus.SingleOrDefault(s => s.default_status);
            if (status == null)
            {
                Session["FlashMessage"] = "Appointment default status not found. Please go to Configuration->Appointment->Edit Status to configure.";
                return RedirectToAction("Index");
            }
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else if (User.IsInRole("ProgramAdmin"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.UserProfiles1.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            return View();
        }

        //
        // POST: /Appointment/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppointmentCreateMultipleViewModel ViewModel)
        {
            Appointment appointment = ViewModel.appointment;
            if (ModelState.IsValid)
            {
                if (ViewModel.concerns != null)
                {
                    foreach (var concernid in ViewModel.concerns)
                    {
                        var concern = db.AppointmentConcerns.Find(concernid);
                        if (concern != null)
                        {
                            appointment.AppointmentConcerns.Add(concern);
                        }
                    }
                }
                appointment.end_time = appointment.start_time.AddMinutes(ViewModel.duration);

                foreach (var ex_session in db.Appointments.Where(a => a.host_id == appointment.host_id))
                {
                    DateTime ex_starttime = ex_session.start_time;
                    DateTime ex_endtime = ex_session.end_time;
                    if ((appointment.start_time >= ex_starttime && appointment.start_time < ex_endtime)
                        || (appointment.end_time > ex_starttime && appointment.end_time <= ex_endtime)
                        || (appointment.start_time <= ex_starttime && appointment.end_time >= ex_endtime))
                    {
                        Session["FlashMessage"] = "Failed to create appointment timeslot. Timeslot overlapped.";
                        return RedirectToAction("Index");
                    }
                }

                var student = db.StudentProfiles.Find(appointment.student_id);
                if (student != null)
                {
                    appointment.student_id = student.id;
                }
                else
                {
                    appointment.student_id = null;
                }

                db.Appointments.Add(appointment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var status = db.AppointmentStatus.SingleOrDefault(s => s.default_status);
            if (status == null)
            {
                Session["FlashMessage"] = "Appointment default status not found. Please go to Configuration->Appointment->Edit Status to configure.";
                return RedirectToAction("Index");
            }
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else if (User.IsInRole("ProgramAdmin"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.UserProfiles1.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            return View(appointment);
        }

        //
        // GET: /Interview/CreateTimeslotSession

        [Ajax(true)]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult CreateTimeslotSession(int index)
        {
            ViewBag.index = index;
            return PartialView();
        }

        //
        // GET: /Interview/CreateTimeslotSkippedDate

        [Ajax(true)]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult CreateTimeslotSkippedDate(int index)
        {
            ViewBag.index = index;
            return PartialView();
        }

        //
        // GET: /Interview/CreateMultiple

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult CreateMultiple(bool consultation = false)
        {
            AppointmentCreateMultipleViewModel ViewModel = new AppointmentCreateMultipleViewModel();
            ViewModel.appointment = new Appointment();
            ViewModel.duration = 30;
            var status = db.AppointmentStatus.SingleOrDefault(s => s.default_status);
            if (status == null)
            {
                Session["FlashMessage"] = "Appointment default status not found. Please go to Configuration->Appointment->Edit Status to configure.";
                return RedirectToAction("Index");
            }
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            ViewBag.concernList = new MultiSelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "id", "name");
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else if(User.IsInRole("ProgramAdmin"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.UserProfiles1.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.consultation = consultation;
            return View(ViewModel);
        }

        //
        // POST: /Interview/CreateMultiple

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMultiple(AppointmentCreateMultipleViewModel ViewModel, bool consultation = false)
        {
            Appointment appointment = ViewModel.appointment;
            if (consultation)
            {
                int programid = ViewModel.concerns.First();
                var concern = db.AppointmentConcerns.Where(c => c.program_id == programid).SingleOrDefault();
                if (concern == null)
                {
                    var program = programdb.Programs.Find(programid);
                    if (program != null)
                    {
                        concern = new AppointmentConcern { name = "Consultation", program_id = program.id };
                        db.AppointmentConcerns.Add(concern);
                    }
                }
                appointment.AppointmentConcerns.Add(concern);
            }
            else
            {
                if (ViewModel.concerns != null)
                {
                    foreach (var concernid in ViewModel.concerns)
                    {
                        var concern = db.AppointmentConcerns.Find(concernid);
                        if (concern != null)
                        {
                            appointment.AppointmentConcerns.Add(concern);
                        }
                    }
                }
            }

            var status = db.AppointmentStatus.SingleOrDefault(s => s.default_status);
            if (status == null)
            {
                Session["FlashMessage"] = "Appointment default status not found. Please go to Configuration->Appointment->Edit Status to configure.";
                return RedirectToAction("Index");
            }
            appointment.status_id = status.id;
            List<bool> availableDaysOfWeek = PrepareAvailableDaysOfWeek(ViewModel.config);
            List<Timeslot> timeslots = new List<Timeslot>();
            DateTime datecursor = ViewModel.config.start_date.Date;

            //initialize skipped_dates to avoid null reference 
            var skipped_dates = new List<DateTime>();
            if (ViewModel.skipped_dates != null)
            {
                skipped_dates = ViewModel.skipped_dates.ToList();
            }

            int count = 0;

            try
            {
                while (datecursor <= ViewModel.config.end_date.Date)
                {
                    if (availableDaysOfWeek[Convert.ToInt16(datecursor.DayOfWeek)] && !skipped_dates.Any(d => d.Date == datecursor.Date))
                    {
                        foreach (var session in ViewModel.sessions.Where(s => !s.excluded).ToList())
                        {
                            TimeSpan starttime = session.start_time.TimeOfDay;
                            TimeSpan duration = new TimeSpan(0, ViewModel.duration, 0);
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

                                foreach (var ex_session in db.Appointments.Where(a => a.host_id == appointment.host_id))
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
                                    appointment.start_time = datecursor.Add(starttime);
                                    appointment.end_time = datecursor.Add(endtime);
                                    db.Appointments.Add(appointment);
                                    db.SaveChanges();
                                    count++;
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
                Session["FlashMessage"] = count + " Appointment Timeslots were created successfully.";
                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
                ViewBag.concernList = new MultiSelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
                ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "name", "name");
                if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
                {
                    ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
                }
                else if (User.IsInRole("ProgramAdmin"))
                {
                    ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.UserProfiles1.Any(u => u.UserName == User.Identity.Name)), "id", "name");
                }
                else
                {
                    ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
                }
                ViewBag.consultation = consultation;
                Session["FlashMessage"] = "Failed to create appointment timeslots." + e.Message;
                return View(ViewModel);
            }
        }

        //
        // GET: /Appointment/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult Edit(int id = 0)
        {
            AppointmentCreateMultipleViewModel ViewModel = new AppointmentCreateMultipleViewModel();
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                Session["FlashMessage"] = "Appointment not found.";
                return RedirectToAction("Index");
            }
            ViewModel.appointment = appointment;
            TimeSpan duration = appointment.end_time.TimeOfDay - appointment.start_time.TimeOfDay;
            ViewModel.duration = duration.Minutes;
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name");
            ViewBag.concernList = new MultiSelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name", appointment.AppointmentConcerns.Select(c => c.id));
            ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "name", "name");
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else if (User.IsInRole("ProgramAdmin"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.UserProfiles1.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            return View(ViewModel);
        }

        //
        // POST: /Appointment/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentCreateMultipleViewModel ViewModel)
        {
            Appointment appointment = db.Appointments.Find(ViewModel.appointment.id);
            if (ModelState.IsValid)
            {
                ViewModel.appointment.end_time = ViewModel.appointment.start_time.AddMinutes(ViewModel.duration);

                foreach (var ex_session in db.Appointments.Where(a => a.id != ViewModel.appointment.id && (a.host_id == appointment.host_id)))
                {
                    DateTime ex_starttime = ex_session.start_time;
                    DateTime ex_endtime = ex_session.end_time;
                    if ((ViewModel.appointment.start_time >= ex_starttime && ViewModel.appointment.start_time < ex_endtime)
                        || (ViewModel.appointment.end_time > ex_starttime && ViewModel.appointment.end_time <= ex_endtime)
                        || (ViewModel.appointment.start_time <= ex_starttime && ViewModel.appointment.end_time >= ex_endtime))
                    {
                        Session["FlashMessage"] = "Failed to edit appointment timeslot. Timeslot overlapped.";
                        return RedirectToAction("Index");
                    }
                }
                var student = db.StudentProfiles.Find(ViewModel.appointment.student_id);
                if (student != null)
                {
                    ViewModel.appointment.student_id = student.id;
                }
                else
                {
                    ViewModel.appointment.student_id = null;
                }

                db.Entry(appointment).CurrentValues.SetValues(ViewModel.appointment);
                appointment.AppointmentConcerns.Clear();
                if (ViewModel.concerns != null)
                {
                    foreach (var concernid in ViewModel.concerns)
                    {
                        var concern = db.AppointmentConcerns.Find(concernid);
                        if (concern != null)
                        {
                            appointment.AppointmentConcerns.Add(concern);
                        }
                    }
                }
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name");
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "name", "name");
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else if (User.IsInRole("ProgramAdmin"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.UserProfiles1.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            return View(appointment);
        }

        //
        // GET: /Appointment/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult Delete(int id = 0)
        {
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                Session["FlashMessage"] = "Appointment not found.";
                return RedirectToAction("Index");
            }
            if (appointment.StudentProfile != null)
            {
                Session["FlashMessage"] = "Appointment is attached to existing Student(s).";
                return RedirectToAction("Index");
            }
            return View(appointment);
        }

        //
        // POST: /Appointment/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Appointment appointment = db.Appointments.Find(id);
            appointment.AppointmentConcerns.Clear();
            db.Appointments.Remove(appointment);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        //
        // GET: /Appointment/BatchDelete/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult BatchDelete(string items)
        {
            var i = items.Split('_');
            var appointments = db.Appointments.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            //ViewBag.StatusList = new SelectList(db.ApplicationStatus, "id", "name");
            ViewBag.items = items;
            return PartialView(appointments.ToList());
        }

        //
        // POST: /Appointment/BatchDelete/

        [HttpPost, ActionName("BatchDelete")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,CommTutor")]
        public ActionResult BatchDeleteConfirmed(string items)
        {
            var i = items.Split('_');
            var appointments = db.Appointments.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            foreach (var appointment in appointments.Where(o => o.student_id == null))
            {
                appointment.AppointmentConcerns.Clear();
                db.Appointments.Remove(appointment);
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete appointment sessions. <br/><br/>" + e.Message;
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Booking()
        {
            return View();
        }

        [Ajax(true)]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Booking1(AppointmentBookingViewModel ViewModel)
        {
            var student = db.StudentProfiles.Find(User.Identity.Name);
            if (student == null)
            {
                Session["FlashMessage"] = "Student not found.";
                return RedirectToAction("Booking");
            }
            ViewModel.advisor = db.StudentAdvisors.FirstOrDefault(a => a.student_id == student.id);
            DateTime dt24 = DateTime.Now.AddHours(24);
            ViewModel.hostList = db.AppointmentHosts.Where(h => h.booking 
                && !h.advisor
                && !h.Appointments.Any(o => o.student_id == student.id && o.start_time > DateTime.Now) 
                && h.Appointments.Where(o => o.start_time > dt24 && o.student_id == null).Count() > 0).ToList();
            if (ViewModel.advisor != null)
            {
                string email = ViewModel.advisor.advisor_email;
                var advisorhost = db.AppointmentHosts.FirstOrDefault(h => h.booking
                    && h.advisor
                    && h.SystemUsers.Any(u => u.UserName == email.Substring(0, email.IndexOf("@")))
                    && !h.Appointments.Any(o => o.student_id == student.id && o.start_time > DateTime.Now)
                    && h.Appointments.Where(o => o.start_time > dt24 && o.student_id == null).Count() > 0);
                if (advisorhost != null)
                {
                    if (student.academic_plan_description.Contains("4Y"))
                    {
                        if (student.academic_plan_description.Contains("Undeclared"))
                        {
                            advisorhost.name = "Pre-major Faculty Advisor - " + advisorhost.name;
                        }
                        else
                        {
                            advisorhost.name = "Faculty Mentor - " + advisorhost.name;
                        }
                        ViewModel.hostList.Add(advisorhost);
                    }
                }
            }
            return View(ViewModel);
        }

        [Ajax(true)]
        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Booking2(AppointmentBookingViewModel ViewModel)
        {
            var student = db.StudentProfiles.Find(User.Identity.Name);
            if (student == null)
            {
                Session["FlashMessage"] = "Student not found.";
                return RedirectToAction("Booking");
            }
            ViewModel.host = db.AppointmentHosts.Find(ViewModel.host_id);
            if (ViewModel.host.advisor && student.academic_plan_description.Contains("4Y"))
            {
                if (student.academic_plan_description.Contains("Undeclared"))
                {
                    ViewModel.host.name = "Pre-major Faculty Advisor - " + ViewModel.host.name;
                }
                else
                {
                    ViewModel.host.name = "Faculty Mentor - " + ViewModel.host.name;
                }
            }
            DateTime dt24 = DateTime.Now.AddHours(24);
            ViewModel.appointmentList = db.Appointments.Where(o => o.host_id == ViewModel.host_id 
                && o.student_id == null
                && o.start_time > dt24).OrderBy(o => o.start_time).ToList();
            return View(ViewModel);
        }

        [Ajax(true)]
        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Booking3(AppointmentBookingViewModel ViewModel)
        {
            var student = db.StudentProfiles.Find(User.Identity.Name);
            if (student == null)
            {
                Session["FlashMessage"] = "Student not found.";
                return RedirectToAction("Booking");
            }
            ViewModel.host = db.AppointmentHosts.Find(ViewModel.host_id);
            if (ViewModel.host.advisor && student.academic_plan_description.Contains("4Y"))
            {
                if (student.academic_plan_description.Contains("Undeclared"))
                {
                    ViewModel.host.name = "Pre-major Faculty Advisor - " + ViewModel.host.name;
                }
                else
                {
                    ViewModel.host.name = "Faculty Mentor - " + ViewModel.host.name;
                }
            }
            ViewModel.appointment = db.Appointments.Find(ViewModel.appointment_id);
            ViewModel.concernList = db.AppointmentConcerns.Where(c => !c.custom && c.program_id == null).ToList();
            if (ViewModel.concern_ids == null && ViewModel.appointment.AppointmentConcerns.Count() > 0)
            {
                ViewModel.concern_ids = ViewModel.appointment.AppointmentConcerns.Select(c => c.id).ToArray();
            }
            return View(ViewModel);
        }

        [Ajax(true)]
        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult BookingConfirm(AppointmentBookingViewModel ViewModel)
        {
            var student = db.StudentProfiles.Find(User.Identity.Name);
            if (student == null)
            {
                Session["FlashMessage"] = "Student not found.";
                return RedirectToAction("Booking");
            }
            ViewModel.host = db.AppointmentHosts.Find(ViewModel.host_id);
            if (ViewModel.host.advisor && student.academic_plan_description.Contains("4Y"))
            {
                if (student.academic_plan_description.Contains("Undeclared"))
                {
                    ViewModel.host.name = "Pre-major Faculty Advisor - " + ViewModel.host.name;
                }
                else
                {
                    ViewModel.host.name = "Faculty Mentor - " + ViewModel.host.name;
                }
            }
            ViewModel.appointment = db.Appointments.Find(ViewModel.appointment_id);
            if (ViewModel.concern_ids != null)
            {
                ViewModel.concerns = db.AppointmentConcerns.Where(c => ViewModel.concern_ids.Contains(c.id)).ToList();
            }
            return View(ViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult BookingSubmit(AppointmentBookingViewModel ViewModel)
        {
            ViewModel.host = db.AppointmentHosts.Find(ViewModel.host_id);
            ViewModel.appointment = db.Appointments.Find(ViewModel.appointment_id);
            DateTime dt24 = DateTime.Now.AddHours(24);
            if (ViewModel.appointment.start_time < dt24)
            {
                Session["FlashMessage"] = "Appointment start time within 24 hours.";
                return RedirectToAction("Booking", "Appointment");
            }
            if (ViewModel.concern_ids != null)
            {
                ViewModel.concerns = db.AppointmentConcerns.Where(c => ViewModel.concern_ids.Contains(c.id)).ToList();
            }
            else
            {
                ViewModel.concerns = new List<AppointmentConcern>();
            }

            ViewModel.appointment.AppointmentConcerns.Clear();
            if (!String.IsNullOrEmpty(ViewModel.other_concern))
            {
                var otherconcern = db.AppointmentConcerns.FirstOrDefault(c => c.name == ViewModel.other_concern && c.custom);
                if (otherconcern == null)
                {
                    otherconcern = new AppointmentConcern { name = ViewModel.other_concern, custom = true };
                }
                db.AppointmentConcerns.Add(otherconcern);
                ViewModel.appointment.AppointmentConcerns.Add(otherconcern);
            }
            foreach (var concern in ViewModel.concerns)
            {
                ViewModel.appointment.AppointmentConcerns.Add(concern);
            }
            var student = db.StudentProfiles.Find(User.Identity.Name);
            if (student != null)
            {
                ViewModel.appointment.student_id = student.id;
            }
            try
            {
                db.SaveChanges();
                Session["FlashMessage"] = "Appointment has been successfully booked.";
                SendNotification(CreateNotification("AppointmentReserved", ViewModel.appointment));
                SendNotification(CreateNotification("AppointmentReservedAdvisor", ViewModel.appointment));
                return RedirectToAction("MyAppointment", "Appointment");
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to make the appointment booking.<br/><br/>" + e.Message;
                return RedirectToAction("Booking", "Appointment");
            }
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult MyAppointment(bool viewall = false)
        {
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("FacultyAdvisor") || User.IsInRole("ProgramAdmin"))
            {
                return RedirectToAction("Index", "Appointment");
            }
            var appointments = db.Appointments.Where(o => o.student_id == User.Identity.Name 
                && !o.AppointmentConcerns.Any(c => c.program_id != null)
                && (viewall || o.start_time > DateTime.Now));
            return View(appointments.ToList());
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Cancel(int id = 0)
        {
            var appointment = db.Appointments.Where(o => o.student_id == User.Identity.Name && o.id == id).FirstOrDefault();
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("FacultyAdvisor"))
            {
                appointment = db.Appointments.Find(id);
            }
            else
            {
                DateTime dt24 = DateTime.Now.AddHours(24);
                if (appointment.start_time < dt24)
                {
                    Session["FlashMessage"] = "Appointment start time within 24 hours.";
                    return RedirectToAction("MyAppointment", "Appointment");
                }
            }
            if (appointment == null)
            {
                Session["FlashMessage"] = "Appointment not found";
                return RedirectToAction("MyAppointment", "Appointment");
            }
            return View(appointment);
        }

        [HttpPost, ActionName("Cancel")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,ProgramAdmin,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult CancelConfirmed(int id = 0)
        {
            var appointment = db.Appointments.Where(o => o.student_id == User.Identity.Name && o.id == id).FirstOrDefault();
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("FacultyAdvisor"))
            {
                appointment = db.Appointments.Find(id);
            }
            if (appointment == null)
            {
                Session["FlashMessage"] = "Appointment not found";
                return RedirectToAction("MyAppointment", "Appointment");
            }
            appointment.student_id = null;
            db.SaveChanges();
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("FacultyAdvisor"))
            {
                return RedirectToAction("Index", "Appointment");
            }
            return RedirectToAction("MyAppointment", "Appointment");
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult ProgramAdmin()
        {
            var programadmins = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "ProgramAdmin"));
            return View(programadmins.ToList());
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult EditProgramAdmin(int userid)
        {
            var user = db.SystemUsers.Find(userid);
            ViewBag.hostList = db.AppointmentHosts.Where(h => h.advisor);
            ViewBag.selectedHosts = user.AppointmentHosts.Select(h => h.id).ToArray();
            ViewBag.planList = db.StudentProfiles.Select(s => new { id = s.academic_plan_description, name = s.academic_plan_description }).Distinct().OrderBy(s => s.id);
            ViewBag.selectedPlans = user.UserProfileAcademicPlans.Select(p => p.academic_plan).ToArray();
            return View(user);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult EditProgramAdmin(int userid, int[] host_ids, string[] plans)
        {
            var user = db.SystemUsers.Find(userid);
            ViewBag.hostList = db.AppointmentHosts.Where(h => h.advisor);
            ViewBag.selectedHosts = user.AppointmentHosts.Select(h => h.id).ToArray();
            ViewBag.planList = db.StudentProfiles.Select(s => new { id = s.academic_plan_description, name = s.academic_plan_description }).Distinct().OrderBy(s => s.id);
            ViewBag.selectedPlans = user.UserProfileAcademicPlans.Select(p => p.academic_plan).ToArray();

            user.AppointmentHosts.Clear();
            if (host_ids != null)
            {
                var hosts = db.AppointmentHosts.Where(h => host_ids.Contains(h.id));
                foreach (var host in hosts)
                {
                    user.AppointmentHosts.Add(host);
                }
            }

            user.UserProfileAcademicPlans.Clear();
            if (plans != null)
            {
                foreach (var plan in plans)
                {
                    user.UserProfileAcademicPlans.Add(new UserProfileAcademicPlan { user_id = userid, academic_plan = plan });
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to edit hosts of the program admin user.<br/>" + e.Message;
                return View(user);
            }
            return RedirectToAction("ProgramAdmin");
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult DeleteProgramAdmin()
        {
            return null;
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

        [Ajax(true)]
        public ActionResult GetDefaultVenue(int id = 0)
        {
            var host = db.AppointmentHosts.Find(id);
            if (host != null)
            {
                return Content(host.default_venue);
            }
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}