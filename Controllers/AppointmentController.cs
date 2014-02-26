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

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index(bool programonly = false)
        {
            var appointments = db.Appointments.Include(a => a.AppointmentStatus).Include(a => a.StudentProfile).Include(a => a.AppointmentHost);
            ViewBag.concernList = new SelectList(db.AppointmentConcerns, "id", "name");
            ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name");
            return View(appointments.ToList().Where(a => (true)
                && (!programonly || a.AppointmentConcerns.Any(c => c.program_id != null))
            ));
        }

        //
        // POST: /Appointment/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index(FormCollection Form, bool reserved, bool programonly, bool available)
        {
            var appointments = db.Appointments.Include(a => a.AppointmentStatus).Include(a => a.StudentProfile).Include(a => a.AppointmentHost);
            ViewBag.concernList = new SelectList(db.AppointmentConcerns, "id", "name", Form["concern"]);
            ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name", Form["host"]);
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name", Form["venue"]);
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", Form["status"]);
            return View(appointments.ToList().Where(a => (true)
                && (String.IsNullOrEmpty(Form["concern"]) || a.AppointmentConcerns.Any(c => c.id.ToString() == Form["concern"]))
                && (String.IsNullOrEmpty(Form["host"]) || a.host_id.ToString() == Form["host"])
                && (String.IsNullOrEmpty(Form["venue"]) || a.venue_id.ToString() == Form["venue"])
                && (String.IsNullOrEmpty(Form["status"]) || a.status_id.ToString() == Form["status"])
                && (!reserved || a.student_id != null)
                && (!available || a.student_id == null)
                && (!programonly || a.AppointmentConcerns.Any(c => c.program_id != null))
                ));
        }

        //
        // GET: /Appointment/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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
        // GET: /Appointment/Details/5

        public ActionResult Calendar(int programid = 0)
        {
            var appointments = db.Appointments.Where(a => a.AppointmentConcerns.Any(c => c.program_id == programid));
            return View(appointments.ToList());
        }



        //
        // GET: /Appointment/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create()
        {
            var status = db.AppointmentStatus.Where(s => s.name == "Opened").SingleOrDefault();
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null), "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View();
        }

        //
        // POST: /Appointment/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                db.Appointments.Add(appointment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            var status = db.AppointmentStatus.Where(s => s.name == "Opened").SingleOrDefault();
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            ViewBag.concernList = new SelectList(db.AppointmentConcerns.Where(c => c.program_id == null), "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View(appointment);
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
            AppointmentCreateMultipleViewModel ViewModel = new AppointmentCreateMultipleViewModel();
            ViewModel.appointment = new Appointment();

            var status = db.AppointmentStatus.Where(s => s.name == "Opened").SingleOrDefault();
            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name", status.id);
            ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "id", "name");
            ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View(ViewModel);
        }

        //
        // POST: /Interview/CreateMultiple

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateMultiple(AppointmentCreateMultipleViewModel ViewModel)
        {
            Appointment appointment = ViewModel.appointment;

            var concern = db.AppointmentConcerns.Where(c => c.program_id == ViewModel.concern).SingleOrDefault();
            if (concern == null)
            {
                var program = programdb.Programs.Find(ViewModel.concern);
                if (program != null)
                {
                    concern = new AppointmentConcern { name = program.name, program_id = program.id };
                    db.AppointmentConcerns.Add(concern);
                }
                else
                {
                    return HttpNotFound("Program not found.");
                }
            }

            appointment.AppointmentConcerns.Add(concern);

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
                ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "name", "name");
                ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
                ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
                Session["FlashMessage"] = "Failed to create appointment timeslots." + e.Message;
                return View(ViewModel);
            }
        }

        //
        // GET: /Appointment/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                Session["FlashMessage"] = "Appointment not found.";
                return RedirectToAction("Index");
            }

            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name");
            ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "name", "name");
            ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View(appointment);
        }

        //
        // POST: /Appointment/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Appointment appointment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.statusList = new SelectList(db.AppointmentStatus, "id", "name");
            ViewBag.programList = new SelectList(programdb.Programs.Where(p => p.require_appointment), "name", "name");
            ViewBag.hostList = new SelectList(db.AppointmentHosts, "id", "name");
            ViewBag.venueList = new SelectList(db.AppointmentVenues, "id", "name");
            return View(appointment);
        }

        //
        // GET: /Appointment/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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