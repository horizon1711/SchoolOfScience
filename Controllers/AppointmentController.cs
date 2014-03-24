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
    public class AppointmentController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();
        private SchoolOfScienceEntities programdb = new SchoolOfScienceEntities();

        //
        // GET: /Appointment/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
        public ActionResult Index(bool consultation = false)
        {
            IQueryable<Appointment> appointments;
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                appointments = db.Appointments;
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else
            {
                appointments = db.Appointments.Where(a => a.AppointmentHost.SystemUsers.Any(u => u.UserName == User.Identity.Name));
                consultation = false;
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name");
            ViewBag.consultation = consultation;
            return View(appointments.ToList().Where(a => (true)
                && (!consultation || a.AppointmentConcerns.Any(c => c.program_id != null))
            ));
        }

        //
        // POST: /Appointment/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
        public ActionResult Index(FormCollection Form, bool reserved, bool available, bool consultation = false)
        {
            IQueryable<Appointment> appointments;
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                appointments = db.Appointments;
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name", Form["host"]);
            }
            else
            {
                appointments = db.Appointments.Where(a => a.AppointmentHost.SystemUsers.Any(u => u.UserName == User.Identity.Name));
                consultation = false;
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name", Form["host"]);
            }
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name", Form["concern"]);
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name", Form["venue"]);
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", Form["status"]);
            ViewBag.consultation = consultation;
            return View(appointments.ToList().Where(a => (true)
                && (String.IsNullOrEmpty(Form["concern"]) || a.AppointmentConcerns.Any(c => c.id.ToString() == Form["concern"]))
                && (String.IsNullOrEmpty(Form["host"]) || a.host_id.ToString() == Form["host"])
                && (String.IsNullOrEmpty(Form["venue"]) || a.venue_id.ToString() == Form["venue"])
                && (String.IsNullOrEmpty(Form["status"]) || a.status_id.ToString() == Form["status"])
                && (!reserved || a.student_id != null)
                && (!available || a.student_id == null)
                && (!consultation || a.AppointmentConcerns.Any(c => c.program_id != null))
                ));
        }

        //
        // GET: /Appointment/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
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
            var appointments = db.Appointments.Where(a => a.host_id == host_id);
            return View(appointments.ToList());
        }

        //
        // GET: /Appointment/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
        public ActionResult Create()
        {
            var status = db.AppointmentStatus.Where(s => s.name == "Opened").SingleOrDefault();
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View();
        }

        //
        // POST: /Appointment/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
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
                db.Appointments.Add(appointment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var status = db.AppointmentStatus.Where(s => s.name == "Opened").SingleOrDefault();
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View(appointment);
        }

        //
        // GET: /Interview/CreateTimeslotSession

        [Ajax(true)]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
        public ActionResult CreateTimeslotSession(int index)
        {
            ViewBag.index = index;
            return PartialView();
        }

        //
        // GET: /Interview/CreateTimeslotSkippedDate

        [Ajax(true)]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
        public ActionResult CreateTimeslotSkippedDate(int index)
        {
            ViewBag.index = index;
            return PartialView();
        }

        //
        // GET: /Interview/CreateMultiple

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
        public ActionResult CreateMultiple(bool consultation = false)
        {
            AppointmentCreateMultipleViewModel ViewModel = new AppointmentCreateMultipleViewModel();
            ViewModel.appointment = new Appointment();
            ViewModel.duration = 30;
            var status = db.AppointmentStatus.Where(s => s.name == "Opened").SingleOrDefault();
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            ViewBag.concernList = new MultiSelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
            ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "id", "name");
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            }
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            ViewBag.consultation = consultation;
            return View(ViewModel);
        }

        //
        // POST: /Interview/CreateMultiple

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
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
            
            int openedStatusId = db.AppointmentStatus.Where(x => x.name == "Opened").FirstOrDefault().id;
            appointment.status_id = openedStatusId;
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

                                foreach (var ex_session in db.Appointments.Where(a => a.venue_id == appointment.venue_id))
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
                var status = db.AppointmentStatus.Where(s => s.name == "Opened").SingleOrDefault();
                ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
                ViewBag.concernList = new MultiSelectList(db.AppointmentConcerns.Where(c => c.program_id == null && !c.custom), "id", "name");
                ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "name", "name");
                if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment"))
                {
                    ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
                }
                else
                {
                    ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
                }
                ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
                ViewBag.consultation = consultation;
                Session["FlashMessage"] = "Failed to create appointment timeslots." + e.Message;
                return View(ViewModel);
            }
        }

        //
        // GET: /Appointment/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
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
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View(ViewModel);
        }

        //
        // POST: /Appointment/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentCreateMultipleViewModel ViewModel)
        {
            Appointment appointment = db.Appointments.Find(ViewModel.appointment.id);
            if (ModelState.IsValid)
            {
                ViewModel.appointment.end_time = ViewModel.appointment.start_time.AddMinutes(ViewModel.duration);
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
            else
            {
                ViewBag.hostList = new SelectList(db.AppointmentHosts.Where(h => h.SystemUsers.Any(u => u.UserName == User.Identity.Name)), "id", "name");
            }
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View(appointment);
        }

        //
        // GET: /Appointment/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
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
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
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

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
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
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor")]
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
            ViewModel.advisor = db.StudentAdvisors.FirstOrDefault(a => a.student_id == User.Identity.Name);
            ViewModel.hostList = db.AppointmentHosts.Where(h => h.booking && !h.Appointments.Any(o => o.student_id == User.Identity.Name && o.start_time > DateTime.Now) && h.Appointments.Count() > 0).ToList();
            if (ViewModel.advisor != null)
            {
                string email = ViewModel.advisor.advisor_email;
                var advisorhost = db.AppointmentHosts.FirstOrDefault(h => h.SystemUsers.Any(u => u.UserName == email.Substring(0, email.IndexOf("@"))) && !h.Appointments.Any(o => o.student_id == User.Identity.Name && o.start_time > DateTime.Now));
                if (advisorhost != null)
                {
                    ViewModel.hostList.Add(advisorhost);
                }
            }
            return View(ViewModel);
        }

        [Ajax(true)]
        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Booking2(AppointmentBookingViewModel ViewModel)
        {
            ViewModel.host = db.AppointmentHosts.Find(ViewModel.host_id);
            ViewModel.appointmentList = db.Appointments.Where(o => o.AppointmentStatus.name == "Opened" && o.host_id == ViewModel.host_id && o.student_id == null).ToList();
            return View(ViewModel);
        }

        [Ajax(true)]
        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Booking3(AppointmentBookingViewModel ViewModel)
        {
            ViewModel.host = db.AppointmentHosts.Find(ViewModel.host_id);
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
            ViewModel.host = db.AppointmentHosts.Find(ViewModel.host_id);
            ViewModel.appointment = db.Appointments.Find(ViewModel.appointment_id);
            ViewModel.concerns = db.AppointmentConcerns.Where(c => ViewModel.concern_ids.Contains(c.id)).ToList();
            return View(ViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult BookingSubmit(AppointmentBookingViewModel ViewModel)
        {
            ViewModel.host = db.AppointmentHosts.Find(ViewModel.host_id);
            ViewModel.appointment = db.Appointments.Find(ViewModel.appointment_id);
            ViewModel.concerns = db.AppointmentConcerns.Where(c => ViewModel.concern_ids.Contains(c.id)).ToList();

            ViewModel.appointment.AppointmentConcerns.Clear();
            if (!String.IsNullOrEmpty(ViewModel.other_concern))
            {
                var otherconcern = new AppointmentConcern { name = ViewModel.other_concern, custom = true };
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
                return RedirectToAction("MyAppointment", "Appointment");
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to make the appointment booking.<br/><br/>" + e.Message;
                return RedirectToAction("Booking", "Appointment");
            }
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult MyAppointment()
        {
            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("FacultyAdvisor"))
            {
                return RedirectToAction("Index", "Appointment");
            }
            var appointments = db.Appointments.Where(o => o.student_id == User.Identity.Name);
            return View(appointments.ToList());
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Cancel(int id = 0)
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
            return View(appointment);
        }

        [HttpPost, ActionName("Cancel")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,FacultyAdvisor,StudentUGRD,StudentRPGTPG,StudentNUGD")]
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

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}