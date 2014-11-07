using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;
using SchoolOfScience.Models;
using SchoolOfScience.Models.ViewModels;
using System.Data.Objects.SqlClient;
using SchoolOfScience.Attributes;
using System.IO;
using Ionic.Zip;

namespace SchoolOfScience.Controllers
{
    public class ApplicationController : ControllerBase
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();
        //
        // GET: /Application/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor")]
        public ActionResult Index(int programId = 0)
        {
            var applications = db.Applications.Include(a => a.Program).Include(a => a.StudentProfile);
            if (programId != 0)
            {
                applications = applications.Where(a => a.program_id == programId);
            }
            var programs = db.Programs.Include(p => p.ProgramStatus).Include(p => p.ProgramType);
            if (User.IsInRole("EDP"))
            {
                var edpusers = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "EDP")).Select(u => u.UserName).ToArray();
                applications = applications.Where(i => edpusers.Contains(i.Program.created_by));
                programs = programs.Where(p => edpusers.Contains(p.created_by));
            }
            if (User.IsInRole("CommTutor"))
            {
                var commtutorusers = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "CommTutor")).Select(u => u.UserName).ToArray();
                applications = applications.Where(i => commtutorusers.Contains(i.Program.created_by));
                programs = programs.Where(p => commtutorusers.Contains(p.created_by));
            }
            ViewBag.programList = new SelectList(programs.Where(p => p.Applications.Count() > 0).OrderBy(p => p.name), "id", "name", programId);
            ViewBag.programTypeList = new SelectList(db.ProgramTypes, "id", "name");
            ViewBag.programStatusList = new SelectList(db.ProgramStatus, "id", "name");
            ViewBag.applicationStatusList = new SelectList(db.ApplicationStatus, "id", "name");
            return View(applications.ToList());
        }
        //
        // GET: /Application/

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor")]
        public ActionResult Index(FormCollection Form, bool interview, bool appointment, bool exchange, bool nomination)
        {
            var applications = db.Applications.Include(a => a.Program).Include(a => a.StudentProfile);
            var programs = db.Programs.Include(p => p.ProgramStatus).Include(p => p.ProgramType);
            if (User.IsInRole("EDP"))
            {
                var edpusers = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "EDP")).Select(u => u.UserName).ToArray();
                applications = applications.Where(i => edpusers.Contains(i.Program.created_by));
                programs = programs.Where(p => edpusers.Contains(p.created_by));
            }
            if (User.IsInRole("CommTutor"))
            {
                var commtutorusers = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "CommTutor")).Select(u => u.UserName).ToArray();
                applications = applications.Where(i => commtutorusers.Contains(i.Program.created_by));
                programs = programs.Where(p => commtutorusers.Contains(p.created_by));
            }
            ViewBag.programList = new SelectList(programs.Where(p => p.Applications.Count() > 0).OrderBy(p => p.name), "id", "name", Form["program"]);
            ViewBag.programTypeList = new SelectList(db.ProgramTypes, "id", "name", Form["program_type"]);
            ViewBag.programStatusList = new SelectList(db.ProgramStatus, "id", "name", Form["program_status"]);
            ViewBag.applicationStatusList = new SelectList(db.ApplicationStatus, "id", "name", Form["application_status"]);
            return View(applications.ToList().Where(a => (true)
                    && (String.IsNullOrEmpty(Form["program"]) || a.Program.id.ToString() == Form["program"])
                    && (String.IsNullOrEmpty(Form["program_type"]) || a.Program.type_id.ToString() == Form["program_type"])
                    && (String.IsNullOrEmpty(Form["program_status"]) || a.Program.status_id.ToString() == Form["program_status"])
                    && (String.IsNullOrEmpty(Form["application_status"]) || a.status_id.ToString() == Form["application_status"])
                    && (!interview || a.Program.require_interview)
                    && (!appointment || a.Program.require_appointment)
                    && (!exchange || a.Program.require_exchange_option)
                    && (!nomination || a.Program.require_nomination)
                ));
        }

        //
        // GET: /Application/Record/

        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Record(int id = 0)
        {
            IQueryable<Application> applications;
            if (id == 1)
            {
                applications = db.Applications.Where(a => a.created_by == User.Identity.Name && a.Program.require_interview && a.Interviews.Where(i => i.InterviewStatus.display_to_student).Count() > 0);
                ViewBag.title = "My Interviews";
                ViewBag.norecord = "interview";
            }
            else if (id == 2)
            {
                applications = db.Applications.Where(a => a.student_id == User.Identity.Name && a.Program.require_appointment && a.Program.AppointmentConcerns.Any(c => c.Appointments.Any(p => p.student_id == User.Identity.Name)));
                ViewBag.title = "My Consultations";
                ViewBag.norecord = "consultation";
            }
            else
            {
                applications = db.Applications.Where(a => a.created_by == User.Identity.Name);
                ViewBag.title = "My Applications";
                ViewBag.norecord = "application";
            }
            List<ProgramAction> programactions = new List<ProgramAction>();
            foreach (var application in applications)
            {
                ProgramAction programaction = new ProgramAction();
                programaction.application = application;
                programaction.program = application.Program;
                programaction.student = application.StudentProfile;

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

            return View(programactions.ToList());
        }

        //
        // GET: /Application/Interview/

        //[Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        //public ActionResult Interview()
        //{
        //    var applications = db.Applications.Include(a => a.Program).Include(a => a.StudentProfile).Where(a => a.created_by == User.Identity.Name && a.Program.require_interview && a.Interviews.Count() > 0);
        //    List<ProgramAction> programactions = new List<ProgramAction>();
        //    foreach (var application in applications)
        //    {
        //        ProgramAction programaction = new ProgramAction();
        //        programaction.application = application;
        //        programaction.program = application.Program;
        //        programaction.student = application.StudentProfile;

        //        //check student eligibility
        //        programaction.eligible = true;
        //        //check program application period
        //        programaction.inperiod = (application.Program.application_start_time <= DateTime.Now && DateTime.Now <= application.Program.application_end_time);
        //        //check if before program start time
        //        programaction.beforestart = (DateTime.Now < application.Program.application_start_time);
        //        //check existing application
        //        programaction.existed = true;
        //        //check application status
        //        programaction.saved = programaction.application.ApplicationStatus.editable;
        //        //check program status
        //        programaction.open = application.Program.ProgramStatus.open_for_application;
        //        programactions.Add(programaction);
        //    }

        //    return View(programactions.ToList());
        //}

        ////
        //// GET: /Application/Appointment/

        //[Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        //public ActionResult Appointment()
        //{
        //    var applications = db.Applications.Where(a => a.student_id == User.Identity.Name && a.Program.AppointmentConcerns.Any(c => c.Appointments.Any(p => p.student_id == User.Identity.Name)));
        //    List<ProgramAction> programactions = new List<ProgramAction>();
        //    foreach (var application in applications)
        //    {
        //        ProgramAction programaction = new ProgramAction();
        //        programaction.application = application;
        //        programaction.program = application.Program;
        //        programaction.student = application.StudentProfile;

        //        //check student eligibility
        //        programaction.eligible = true;
        //        //check program application period
        //        programaction.inperiod = (application.Program.application_start_time <= DateTime.Now && DateTime.Now <= application.Program.application_end_time);
        //        //check if before program start time
        //        programaction.beforestart = (DateTime.Now < application.Program.application_start_time);
        //        //check existing application
        //        programaction.existed = true;
        //        //check application status
        //        programaction.saved = programaction.application.ApplicationStatus.editable;
        //        //check program status
        //        programaction.open = application.Program.ProgramStatus.open_for_application;
        //        programactions.Add(programaction);
        //    }

        //    return View(programactions.ToList());
        //}

        //
        // GET: /Application/Details/5
        [Ajax(true)]
        [Authorize]
        public ActionResult Details(int id = 0, int listid = 0, bool fulldetails = false)
        {
            ViewBag.fulldetails = fulldetails;
            Application application = db.Applications.Find(id);
            if (application == null)
            {
                return HttpNotFound("Application not found.");
            }

            Program program = db.Programs.Find(application.program_id);
            if (program == null)
            {
                return HttpNotFound("Program not found.");
            }

            ApplicationViewModel ViewModel = new ApplicationViewModel();
            ViewModel.application = application;
            ViewModel.program = program;
            ViewModel.student = application.StudentProfile;

            ViewModel.options = new List<ApplicationOptionValue>();
            application.ApplicationOptionValues.ToList().ForEach(o => ViewModel.options.Add(o));

            ViewModel.attachments = new List<ApplicationAttachment>();
            application.ApplicationAttachments.ToList().ForEach(a => ViewModel.attachments.Add(a));

            ViewModel.exchange_options = new List<ApplicationExchangeOption>();
            if (program.require_exchange_option)
            {
                ViewBag.ExchangeOptionList = new SelectList(db.ExchangeOptions.Where(e => e.status), "id", "name");
                application.ApplicationExchangeOptions.OrderBy(e => e.priority).ToList().ForEach(e => ViewModel.exchange_options.Add(e));
            }


            if (program.require_appointment)
            {
                var app = db.Appointments.Where(a => a.student_id == application.student_id && a.AppointmentConcerns.Any(c => c.program_id == program.id)).FirstOrDefault();
                if (app != null)
                {
                    ViewBag.appointment_start_time = app.start_time;
                }
            }

            if (listid != 0)
            {
                var list = db.NominationLists.Find(listid);
                var nominated_application = list.NominationApplications.SingleOrDefault(a => a.application_id == application.id);
                if (nominated_application != null)
                {
                    ViewModel.nominated_application = nominated_application;
                }
                else
                {
                    ViewModel.nominated_application = new NominationApplication
                    {
                        nomination_list_id = list.id,
                        application_id = application.id,
                        nomination_id = list.nomination_id
                    };
                }
            }

            return PartialView(ViewModel);
        }

        //
        // GET: /Application/Create
        [Ajax(true)]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Create(int id = 0)
        {
            Program program = db.Programs.Find(id);
            if (program == null)
            {
                return HttpNotFound("Program not found.");
            }

            StudentProfile student = db.StudentProfiles.Find(User.Identity.Name);
            if (student == null)
            {
                return HttpNotFound("Student not found.");
            }

            if (!IsEligible(program, student))
            {
                return HttpNotFound("Not eligible to apply this program.");
            }

            if (program.application_end_time < DateTime.Now)
            {
                return HttpNotFound("Application deadline expired.");
            }
            var SavedStatus = db.ApplicationStatus.FirstOrDefault(s => s.editable);
            var SubmittedStatus = db.ApplicationStatus.FirstOrDefault(s => s.submitted);
            if (SavedStatus == null || SubmittedStatus == null)
            {
                Session["FlashMessage"] = "Saved or Submitted Status not found.";
                return RedirectToAction("Record");
            }
            else
            {
                ViewBag.savedStatus_id = SavedStatus.id;
                ViewBag.submittedStatus_id = SubmittedStatus.id;
            }

            Application application = db.Applications.Where(a => (a.student_id == student.id && a.program_id == program.id)).FirstOrDefault();
            if (application != null)
            {
                return RedirectToAction("Edit", "Application", new { id = application.id });
            }

            ApplicationViewModel ViewModel = new ApplicationViewModel();
            ViewModel.application = new Application();
            ViewModel.application.program_id = program.id;
            ViewModel.application.student_id = student.id;
            ViewModel.program = program;
            ViewModel.student = student;

            ViewModel.options = new List<ApplicationOptionValue>();
            foreach (var option in program.ProgramOptionValues)
            {
                ViewModel.options.Add(new ApplicationOptionValue { 
                    option_value_id = option.id,
                    ProgramOptionValue = option
                });
            }

            ViewModel.attachments = new List<ApplicationAttachment>();
            foreach (var attachment in program.ProgramApplicationAttachments)
            {
                ViewModel.attachments.Add(new ApplicationAttachment
                {
                    program_application_attachment_id = attachment.id,
                    ProgramApplicationAttachment = attachment
                });
            }

            ViewModel.exchange_options = new List<ApplicationExchangeOption>();
            if (program.require_exchange_option)
            {
                ViewBag.ExchangeOptionList = new SelectList(db.ExchangeOptions.Where(e => e.status).OrderBy(e => e.name), "id", "name");
                for (int i = 0; i < ViewModel.no_of_exchange_options; i++)
                {
                    ViewModel.exchange_options.Add(new ApplicationExchangeOption{
                        priority = i
                    });
                }
            }

            if (program.require_appointment)
            {
                ViewBag.AppointmentList = db.Appointments.Where(a => a.student_id == null && a.AppointmentConcerns.Any(c => c.program_id == program.id)).OrderBy(a => a.start_time);
            }

            return PartialView(ViewModel);
        }

        //
        // POST: /Application/Create

        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ApplicationViewModel ViewModel)
        {
            Application application = ViewModel.application;
            Program program = db.Programs.Find(application.program_id);
            StudentProfile student = db.StudentProfiles.Find(application.student_id);

            if (!IsEligible(program, student))
            {
                Session["FlashMessage"] = "Not eligible to apply.";
                return RedirectToAction("Record");
            }

            if (DateTime.Now > program.application_end_time)
            {
                Session["FlashMessage"] = "Application deadline has passed. ";
                return RedirectToAction("Record");
            }

            var SavedStatus = db.ApplicationStatus.FirstOrDefault(s => s.editable);
            var SubmittedStatus = db.ApplicationStatus.FirstOrDefault(s => s.submitted);
            if (SavedStatus == null || SubmittedStatus == null)
            {
                Session["FlashMessage"] = "Saved or Submitted Status not found.";
                return RedirectToAction("Record");
            }
            else
            {
                ViewBag.savedStatus_id = SavedStatus.id;
                ViewBag.submittedStatus_id = SubmittedStatus.id;
            }

            //201403241714 fai: prevent duplicated application
            if (student.Applications.Any(a => a.program_id == program.id))
            {
                Session["FlashMessage"] = "Application of [" + program.name + "] already exists. You may Edit/View your application from the list.";
                return RedirectToAction("Record");
            }

            if (ModelState.IsValid)
            {
                application.created = DateTime.Now;
                application.created_by = User.Identity.Name;
                application.modified = DateTime.Now;
                application.modified_by = User.Identity.Name;
                if (db.ApplicationStatus.Any(s => s.id == application.status_id && s.submitted))
                {
                    application.submitted = DateTime.Now;
                }
                db.Applications.Add(application);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create application. " + e.Message;
                }

                foreach (var item in program.ProgramOptionValues)
                {
                    ApplicationOptionValue option = ViewModel.options.Where(o => o.option_value_id == item.id).SingleOrDefault();
                    option.application_id = application.id;
                    db.ApplicationOptionValues.Add(option);
                }

                foreach (var item in program.ProgramApplicationAttachments) 
                {
                    ApplicationAttachment attachment = ViewModel.attachments.Where(a => a.program_application_attachment_id == item.id).SingleOrDefault();
                    attachment.application_id = application.id;

                    if (!String.IsNullOrEmpty(attachment.filename))
                    {
                        var sourcePath = Server.MapPath("~/App_Data/" + attachment.filepath);
                        var sourceFilepath = Path.Combine(sourcePath, attachment.filename);
                        var destPath = Server.MapPath("~/App_Data/" + "Attachments/Application/" + application.id);
                        var destFilepath = Path.Combine(destPath, attachment.filename);
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
                            attachment.filepath = "Attachments/Application/" + application.id;
                        }
                        catch (Exception e)
                        {
                            Session["FlashMessage"] = "Failed to move file." + e.Message;
                        }

                    }

                    db.ApplicationAttachments.Add(attachment);
                }

                //clear temp files uploaded but not used
                if (Directory.Exists(Server.MapPath("~/App_Data/Temp/Application/" + User.Identity.Name)))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Temp/Application/" + User.Identity.Name));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }

                if (program.require_exchange_option)
                {
                    for (int i = 0; i < ViewModel.no_of_exchange_options; i++)
                    {
                        ApplicationExchangeOption exchange_option = ViewModel.exchange_options.Where(e => e.priority == i).SingleOrDefault();
                        exchange_option.application_id = application.id;
                        db.ApplicationExchangeOptions.Add(exchange_option);
                    }
                }

                if (program.require_appointment)
                {
                    if (db.ApplicationStatus.Any(s => s.id == application.status_id && s.submitted))
                    {
                        var appointment = db.Appointments.Find(ViewModel.application.appointment_id);
                        var advising_student = db.StudentProfiles.Find(application.student_id);
                        if (advising_student != null)
                        {
                            if (appointment.student_id == null)
                            {
                                appointment.student_id = advising_student.id;
                            }
                            else
                            {
                                Session["FlashMessage"] = "The consultation session that you have selected is no longer available. Please select another session. <br/><br/>Your application is not submitted";
                                application.status_id = SavedStatus.id;
                            }
                        }
                        db.SaveChanges();
                    }
                }

                try
                {
                    db.SaveChanges();
                    if (db.ApplicationStatus.Any(s => s.id == application.status_id && s.editable))
                    {
                        if (Session["FlashMessage"] == null)
                        {
                            Session["FlashMessage"] = "Application saved successfully.";
                        }
                    }
                    if (db.ApplicationStatus.Any(s => s.id == application.status_id && s.submitted))
                    {
                        if (Session["FlashMessage"] == null)
                        {
                            Session["FlashMessage"] = "Application submitted successfully.";

                            SendNotification(CreateNotification("ApplicationSubmitted", application));
                        }
                    }
                    return RedirectToAction("Record");
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to save Attachment/Optional Field to database." + e.Message;
                    return RedirectToAction("Record");
                }
            }

            Session["FlashMessage"] = "Failed to create application.";
            return RedirectToAction("Record");
        }

        //
        // GET: /Application/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Edit(int id = 0)
        {
            bool authorizedToEdit = User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("EDP") || User.IsInRole("CommTutor");
            Application application = db.Applications.Where(a => (a.created_by == User.Identity.Name || authorizedToEdit) && (a.id == id)).FirstOrDefault();
            if (application == null)
            {
                return HttpNotFound("Application not found.");
            }

            if (db.ApplicationStatus.Any(s => s.id == application.status_id && !s.editable && !authorizedToEdit))
            {
                return HttpNotFound("Application exists but not allow to be edited.");
            }

            Program program = db.Programs.Find(application.program_id);
            if (program == null)
            {
                return HttpNotFound("Program not found.");
            }

            var SavedStatus = db.ApplicationStatus.FirstOrDefault(s => s.editable);
            var SubmittedStatus = db.ApplicationStatus.FirstOrDefault(s => s.submitted);
            if (SavedStatus == null || SubmittedStatus == null)
            {
                Session["FlashMessage"] = "Saved or Submitted Status not found.";
                return RedirectToAction("Record");
            }
            else
            {
                ViewBag.savedStatus_id = SavedStatus.id;
                ViewBag.submittedStatus_id = SubmittedStatus.id;
            }
            
            ApplicationViewModel ViewModel = new ApplicationViewModel();
            ViewModel.application = application;
            ViewModel.program = program;
            ViewModel.student = application.StudentProfile;

            ViewModel.options = new List<ApplicationOptionValue>();
            foreach (var item in program.ProgramOptionValues)
            {
                var option = application.ApplicationOptionValues.Where(o => o.option_value_id == item.id).SingleOrDefault();
                if (option == null)
                {
                    option = new ApplicationOptionValue
                    {
                        option_value_id = item.id,
                        ProgramOptionValue = item,
                        application_id = application.id
                    };
                }
                ViewModel.options.Add(option);
            }

            ViewModel.attachments = new List<ApplicationAttachment>();
            foreach (var item in program.ProgramApplicationAttachments)
            {
                var attachment = application.ApplicationAttachments.Where(a => a.program_application_attachment_id == item.id).SingleOrDefault();
                if (attachment == null)
                {
                    attachment = new ApplicationAttachment
                    {
                        program_application_attachment_id = item.id,
                        ProgramApplicationAttachment = item,
                        application_id = application.id
                    };
                }
                ViewModel.attachments.Add(attachment);
            }

            ViewModel.exchange_options = new List<ApplicationExchangeOption>();
            if (program.require_exchange_option)
            {
                ViewBag.ExchangeOptionList = new SelectList(db.ExchangeOptions.Where(e => e.status).OrderBy(e => e.name), "id", "name");
                for (int i = 0; i < ViewModel.no_of_exchange_options; i++)
                {
                    var exchangeoption = application.ApplicationExchangeOptions.Where(e => e.priority == i).SingleOrDefault();
                    if (exchangeoption == null)
                    {
                        exchangeoption = new ApplicationExchangeOption
                        {
                            application_id = application.id,
                            priority = i
                        };
                    }
                    ViewModel.exchange_options.Add(exchangeoption);
                }
            }

            if (program.require_appointment)
            {
                //application appointment_id exist, but already booked by others
                if (db.Appointments.Any(o => o.student_id != null && o.id == ViewModel.application.appointment_id))
                {
                    ViewBag.booked = "Y";
                    ViewModel.application.appointment_id = null;
                }
                ViewBag.AppointmentList = db.Appointments.Where(a => a.student_id == null && a.AppointmentConcerns.Any(c => c.program_id == program.id));
            }

            return PartialView(ViewModel);
        }

        //
        // POST: /Application/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationViewModel ViewModel)
        {
            Application application = db.Applications.Find(ViewModel.application.id);
            Program program = application.Program;
            StudentProfile student = application.StudentProfile;

            bool authorizedToEdit = User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("EDP") || User.IsInRole("CommTutor");
            if (DateTime.Now > program.application_end_time && !authorizedToEdit)
            {
                Session["FlashMessage"] = ("Application deadline passed. ");
                return RedirectToAction("Record");
            }

            db.Entry(application).CurrentValues.SetValues(ViewModel.application);

            var SavedStatus = db.ApplicationStatus.FirstOrDefault(s => s.editable);
            var SubmittedStatus = db.ApplicationStatus.FirstOrDefault(s => s.submitted);
            if (SavedStatus == null || SubmittedStatus == null)
            {
                Session["FlashMessage"] = "Saved or Submitted Status not found.";
                return RedirectToAction("Record");
            }
            else
            {
                ViewBag.savedStatus_id = SavedStatus.id;
                ViewBag.submittedStatus_id = SubmittedStatus.id;
            }

            if (ModelState.IsValid || application.status_id == SavedStatus.id)
            {
                application.modified = DateTime.Now;
                application.modified_by = User.Identity.Name;
                if (application.status_id == SubmittedStatus.id)
                {
                    application.submitted = DateTime.Now;
                }

                foreach (var item in program.ProgramOptionValues)
                {
                    ApplicationOptionValue option = ViewModel.options.Where(o => o.option_value_id == item.id).SingleOrDefault();
                    if (option.id == 0)
                    {
                        db.ApplicationOptionValues.Add(option);
                    }
                    else
                    {
                        db.Entry(option).State = EntityState.Modified;
                    }
                }

                foreach (var item in program.ProgramApplicationAttachments)
                {
                    ApplicationAttachment attachment = ViewModel.attachments.Where(a => a.program_application_attachment_id == item.id).SingleOrDefault();
                    if (attachment.id == 0) //Application Attachment not exist in DB
                    {
                        if (!String.IsNullOrEmpty(attachment.filename))
                        {
                            var sourcePath = Server.MapPath("~/App_Data/" + attachment.filepath);
                            var sourceFilepath = Path.Combine(sourcePath, attachment.filename);
                            var destPath = Server.MapPath("~/App_Data/" + "Attachments/Application/" + application.id);
                            var destFilepath = Path.Combine(destPath, attachment.filename);
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
                                attachment.filepath = "Attachments/Application/" + application.id;
                            }
                            catch (Exception e)
                            {
                                Session["FlashMessage"] = "Failed to move file." + e.Message;
                            }

                        }
                        db.ApplicationAttachments.Add(attachment);
                    }
                    else //Application Attachment exists in DB
                    {
                        ApplicationAttachment att = db.ApplicationAttachments.Find(attachment.id);
                        if (att.filename != attachment.filename || att.filepath != attachment.filepath)
                        {
                            if (!String.IsNullOrEmpty(att.filename)) //delete only if current file exist
                            {
                                var path = Server.MapPath("~/App_Data/" + att.filepath);
                                var filepath = Path.Combine(path, att.filename);
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

                            if (!String.IsNullOrEmpty(attachment.filename)) //move uploaded file
                            {
                                var sourcePath = Server.MapPath("~/App_Data/" + attachment.filepath);
                                var sourceFilepath = Path.Combine(sourcePath, attachment.filename);
                                var destPath = Server.MapPath("~/App_Data/" + "Attachments/Application/" + application.id);
                                var destFilepath = Path.Combine(destPath, attachment.filename);
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
                                    attachment.filepath = "Attachments/Application/" + application.id;
                                }
                                catch (Exception e)
                                {
                                    Session["FlashMessage"] = "Failed to move file." + e.Message;
                                }
                            }
                        }
                        db.Entry(att).CurrentValues.SetValues(attachment);
                    }
                }

                //clear temp files uploaded but not used
                if (Directory.Exists(Server.MapPath("~/App_Data/Temp/Application/" + application.id)))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Temp/Application/" + application.id));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }

                if (program.require_exchange_option)
                {
                    for (int i = 0; i < ViewModel.no_of_exchange_options; i++)
                    {
                        var exchangeoption = ViewModel.exchange_options.SingleOrDefault(e => e.priority == i);
                        if (exchangeoption.id == 0)
                        {
                            db.ApplicationExchangeOptions.Add(exchangeoption);
                        }
                        else
                        {
                            db.Entry(exchangeoption).State = EntityState.Modified;
                        }
                    }
                }

                if (program.require_appointment)
                {
                    if (SavedStatus != null && application.status_id != SavedStatus.id)
                    {
                        var appointment = db.Appointments.Find(ViewModel.application.appointment_id);
                        var advising_student = db.StudentProfiles.Find(application.student_id);
                        if (advising_student != null)
                        {
                            if (appointment.student_id == null)
                            {
                                appointment.student_id = advising_student.id;
                            }
                            else
                            {
                                Session["FlashMessage"] = "The consultation session that you have selected is no longer available. Please select another session. <br/><br/>***Your application is not submitted";
                                application.status_id = SavedStatus.id;
                            }
                        }
                        db.SaveChanges();
                    }
                }


                try
                {
                    db.SaveChanges();
                    if (db.ApplicationStatus.Any(s => s.id == application.status_id && s.editable))
                    {
                        if (Session["FlashMessage"] == null)
                        {
                            Session["FlashMessage"] = "Application saved successfully.<br/><br/>Should you encounter any technical difficulty, please contact us at <a href='advise@ust.hk'>advise@ust.hk</a>.";
                        }
                    }
                    else if (db.ApplicationStatus.Any(s => s.id == application.status_id && s.submitted))
                    {
                        if (Session["FlashMessage"] == null)
                        {
                            Session["FlashMessage"] = "Application submitted successfully.<br/><br/>Should you encounter any technical difficulty, please contact us at <a href='advise@ust.hk'>advise@ust.hk</a>.";
                            SendNotification(CreateNotification("ApplicationSubmitted", application));
                        }
                    }
                    if (User.IsInRole("Advising"))
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Record");
                    }
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to save modified application to database." + e.Message;
                    if (User.IsInRole("Advising"))
                    {
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("Record");
                    }
                }

            }

            Session["FlashMessage"] = "Failed to edit application.";
            if (User.IsInRole("Advising"))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Record");
            }
        }

        //
        // GET: /Program/BatchUpdate

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor")]
        public ActionResult BatchUpdate(string items)
        {
            var i = items.Split('_');
            var applications = db.Applications.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            ViewBag.StatusList = new SelectList(db.ApplicationStatus, "id", "name");
            ViewBag.items = items;
            return PartialView(applications.ToList());
        }

        //
        // POST: /Program/BatchUpdate
        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor")]
        public ActionResult BatchUpdate(string items, int status_id)
        {
            var i = items.Split('_');
            var applications = db.Applications.Include(a => a.ApplicationStatus).Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            ViewBag.StatusList = new SelectList(db.ApplicationStatus, "id", "name");
            ViewBag.items = items;

            foreach (var application in applications)
            {
                application.status_id = status_id;
                application.modified = DateTime.Now;
                application.modified_by = User.Identity.Name;
                //status backward handling, remove appointment relationship, reset saved appointment_id selection
                if (db.ApplicationStatus.Any(s => s.id == application.status_id && s.editable))
                {
                    var appointments = db.Appointments.Where(a => a.student_id == application.student_id && a.AppointmentConcerns.Any(c => c.program_id == application.program_id));
                    foreach (var item in appointments)
                    {
                        item.student_id = null;
                    }
                    application.appointment_id = null;
                }
            }
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to update database." + e.Message;
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor")]
        public ActionResult BatchDownload(string items)
        {
            var i = items.Split('_');
            var applications = db.Applications.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));

            var savePath = Server.MapPath("~/App_Data/");
            using (ZipFile zip = new ZipFile())
            {
                foreach (var application in applications)
                {
                    foreach (var attachment in application.ApplicationAttachments)
                    {
                        if (!String.IsNullOrEmpty(attachment.filename) && !String.IsNullOrEmpty(attachment.filepath))
                        {
                            var filename = attachment.filename;
                            var path = Server.MapPath("~/App_Data/" + attachment.filepath);
                            var filepath = Path.Combine(path, filename);
                            if (System.IO.File.Exists(filepath))
                            {
                                zip.AddFile(filepath, Path.Combine("ByApplication", application.StudentProfile.name + "[" + application.StudentProfile.academic_organization + "]" + "_" + application.StudentProfile.id + "_" + application.id));
                                //zip.AddFile(filepath, Path.Combine("ByAttachmentType", application.program_id.ToString() + "_" + HttpUtility.UrlEncode(application.Program.name), attachment.ProgramApplicationAttachment.name)).FileName = application.StudentProfile.name + "[" + application.StudentProfile.academic_organization + "]" + "_" + application.StudentProfile.id + "_" + application.id + "_" + filename;
                                zip.AddFile(filepath).FileName = Path.Combine("ByAttachmentType"
                                    , application.program_id.ToString() + "_" + HttpUtility.UrlEncode(application.Program.name)
                                    , attachment.ProgramApplicationAttachment.name
                                    , application.StudentProfile.name + "[" + application.StudentProfile.academic_organization + "]" + "_" + application.StudentProfile.id + "_" + application.id + "_" + filename);
                            }
                        }
                    }
                }
                zip.Save(savePath + "Archive.zip");
            }
            return File(savePath + "Archive.zip", "application/octet-stream", Path.GetFileName(savePath + "Archive.zip"));
        }

        //
        // GET: /Application/Review/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor")]
        public ActionResult Review(int id = 0)
        {
            Application application = db.Applications.Find(id);
            if (application == null)
            {
                return HttpNotFound("Application not found");
            }

            Program program = db.Programs.Find(application.program_id);
            if (program == null)
            {
                return HttpNotFound("Program not found");
            }

            ApplicationViewModel ViewModel = new ApplicationViewModel();
            ViewModel.application = application;
            ViewModel.program = program;
            ViewModel.student = application.StudentProfile;

            ViewModel.options = new List<ApplicationOptionValue>();
            application.ApplicationOptionValues.ToList().ForEach(o => ViewModel.options.Add(o));

            ViewModel.attachments = new List<ApplicationAttachment>();
            application.ApplicationAttachments.ToList().ForEach(a => ViewModel.attachments.Add(a));

            ViewModel.exchange_options = new List<ApplicationExchangeOption>();
            if (program.require_exchange_option)
            {
                ViewBag.ExchangeOptionList = new SelectList(db.ExchangeOptions.Where(e => e.status), "id", "name");
                application.ApplicationExchangeOptions.ToList().ForEach(e => ViewModel.exchange_options.Add(e));
            }

            Interview interview = application.Interviews.FirstOrDefault();
            if (interview != null)
            {
                ViewModel.interview_id = interview.id;
            }
            else
            {
                ViewModel.interview_id = -1;
            }

            if (program.require_appointment)
            {
                var app = db.Appointments.Where(a => a.student_id == application.student_id && a.AppointmentConcerns.Any(c => c.program_id == program.id)).FirstOrDefault();
                if (app != null)
                {
                    ViewBag.appointment_start_time = app.start_time;
                }
            }
            ViewBag.statusList = new SelectList(db.ApplicationStatus, "id", "name");
            ViewBag.interviewList = new SelectList(db.Interviews.Where(i => i.program_id == application.program_id && (i.Applications.Count() < i.no_of_interviewee || i.id == ViewModel.interview_id)), "id", "start_time", ViewModel.interview_id);

            return PartialView(ViewModel);
        }

        //
        // POST: /Application/Review/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor")]
        public ActionResult Review(ApplicationViewModel ViewModel)
        {
            Application application = db.Applications.Find(ViewModel.application.id);
            if (application != null)
            {
                application.modified = DateTime.Now;
                application.modified_by = User.Identity.Name;
                application.status_id = ViewModel.application.status_id;
                //status backward handling, remove appointment relationship, reset saved appointment_id selection
                if (db.ApplicationStatus.Any(s => s.id == application.status_id && s.editable))
                {
                    var appointment = db.Appointments.Where(a => a.student_id == application.student_id && a.AppointmentConcerns.Any(c => c.program_id == application.program_id)).SingleOrDefault();
                    if (appointment != null)
                    {
                        appointment.student_id = null;
                    }
                    application.appointment_id = null;
                    db.SaveChanges();
                    Session["FlashMessage"] += "Related consultation session (if any) is released";
                }
                application.Interviews.Clear();
                application.Interviews.Add(db.Interviews.Find(ViewModel.interview_id));
                try
                {
                    db.Entry(application).State = EntityState.Modified;
                    Session["FlashMessage"] += "<br/>Application Status changed successfully.";
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to update application to database." + e.Message;
                    return View(application);
                }
                return RedirectToAction("Index");
            }
            return View(application);
        }

        //
        // GET: /Application/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,EDP,CommTutor,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Delete(int id = 0)
        {
            Application application = db.Applications.Find(id);
            if (application == null)
            {
                Session["FlashMessage"] = "Application not found.";
                if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("EDP") || User.IsInRole("CommTutor"))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Record");
                }
            }
            var student = db.StudentProfiles.Find(User.Identity.Name);
            bool authorized = User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("EDP") || User.IsInRole("CommTutor");
            if (student == null && !authorized && application.student_id != student.id)
            {
                Session["FlashMessage"] = "Not authorized to delete this application.";
                if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("EDP") || User.IsInRole("CommTutor"))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Record");
                }
            }
            db.ApplicationAttachments.Where(t => t.application_id == application.id).ToList().ForEach(x => db.ApplicationAttachments.Remove(x));
            db.ApplicationExchangeOptions.Where(e => e.application_id == application.id).ToList().ForEach(x => db.ApplicationExchangeOptions.Remove(x));
            db.ApplicationOptionValues.Where(o => o.application_id == application.id).ToList().ForEach(x => db.ApplicationOptionValues.Remove(x));
            db.InterviewComments.Where(o => o.application_id == application.id).ToList().ForEach(x => db.InterviewComments.Remove(x));
            db.Applications.Remove(application);

            try
            {
                db.SaveChanges();
                Session["FlashMessage"] = "Application deleted.";
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete application.<br/>" + e.Message;
                if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("EDP") || User.IsInRole("CommTutor"))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Record");
                }
            }

            try
            {
                //clear attachments uploaded
                if (Directory.Exists(Server.MapPath("~/App_Data/Attachments/Application/" + id)))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Attachments/Application/" + id));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                    Directory.Delete(Server.MapPath("~/App_Data/Attachments/Application/" + id));
                }
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to remove application attachments.<br/>" + e.Message;
            }


            if (User.IsInRole("Admin") || User.IsInRole("Advising") || User.IsInRole("StudentDevelopment") || User.IsInRole("EDP") || User.IsInRole("CommTutor"))
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Record");
            }
        }

        //
        // POST: /Application/Delete/5

        //[Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(ApplicationViewModel ViewModel)
        //{
        //    Application application = db.Applications.Find(ViewModel.application.id);
        //    application.status_id = ViewModel.application.status_id;
        //    try
        //    {
        //        db.Entry(application).State = EntityState.Modified;
        //        db.SaveChanges();
        //    }
        //    catch (Exception e)
        //    {
        //        Session["FlashMessage"] = "Failed to update application delete status." + e.Message;
        //    }
        //    return RedirectToAction("Record");
        //}

        //
        // GET: /Application/CommentTemplate/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult CommentTemplate(string items)
        {
            var i = items.Split('_');
            var applications = db.Applications.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));

            string strHtml = RenderRazorViewToString("CommentTemplate", applications.ToList());
            strHtml = HttpUtility.HtmlDecode(strHtml);//Html解碼
            byte[] b = System.Text.Encoding.UTF8.GetBytes(strHtml);//字串轉byte陣列
            Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            return File(b, "application/vnd.ms-excel", "Application Comment Template " + String.Format("{0:yyyyMMddHHmm}", DateTime.Now) + ".xls");//輸出檔案給Client端
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}