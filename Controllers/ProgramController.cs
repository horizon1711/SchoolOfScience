using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using SchoolOfScience.Models.ViewModels;
using System.Collections;
using SchoolOfScience.Attributes;
using System.IO;
using System.Data.Objects.SqlClient;
using SchoolOfScience.Filters;

namespace SchoolOfScience.Controllers
{
    public class ProgramController : ControllerBase
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /Program/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index()
        {

            var programs = db.Programs.Include(p => p.ProgramStatus).Include(p => p.ProgramType);
            try
            {
                ProgramStatus draft = db.ProgramStatus.Where(m => m.name == "Drafted").FirstOrDefault();
                ViewBag.draftStatusId = draft.id;
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Drafted Status not found." + e.Message;
            }
            ViewBag.programList = new SelectList(db.Programs.OrderBy(p => p.name), "id", "name");
            ViewBag.programTypeList = new SelectList(db.ProgramTypes, "id", "name");
            ViewBag.programStatusList = new SelectList(db.ProgramStatus, "id", "name");
            return View(programs.ToList());
        }

        //
        // POST: /Program/
        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index(FormCollection Form, bool interview, bool appointment, bool exchange, bool nomination)
        {
            var programs = db.Programs.Include(p => p.ProgramStatus).Include(p => p.ProgramType);
            try
            {
                ProgramStatus draft = db.ProgramStatus.Where(m => m.name == "Drafted").FirstOrDefault();
                ViewBag.draftStatusId = draft.id;
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Drafted Status not found." + e.Message;
            }
            ViewBag.programList = new SelectList(db.Programs.OrderBy(p => p.name), "id", "name", Form["program"]);
            ViewBag.programTypeList = new SelectList(db.ProgramTypes, "id", "name", Form["program_type"]);
            ViewBag.programStatusList = new SelectList(db.ProgramStatus, "id", "name", Form["program_status"]);
            return View(programs.ToList().Where(p => (true)
                    && (String.IsNullOrEmpty(Form["program"]) || p.id.ToString() == Form["program"])
                    && (String.IsNullOrEmpty(Form["program_type"]) || p.type_id.ToString() == Form["program_type"])
                    && (String.IsNullOrEmpty(Form["program_status"]) || p.status_id.ToString() == Form["program_status"])
                    && (!interview || p.require_interview)
                    && (!appointment || p.require_appointment)
                    && (!exchange || p.require_exchange_option)
                    && (!nomination || p.require_nomination)
                    ));
        }

        //
        // GET: /Program/Showcase/
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Showcase(int id = 0)
        {
            StudentProfile student = db.StudentProfiles.Find(User.Identity.Name);
            //get program list with status open and not expired. program application start time not reach also shown
            var programs = db.Programs.Where(p => (p.ProgramStatus.name == "Opened") && (DateTime.Now < p.application_end_time));
            List<Program> programList = new List<Program>();
            foreach (var program in programs)
            {
                if (IsEligible(program, student))
                {
                    programList.Add(program);
                }
            }
            //program type list for showcase
            ViewBag.ProgramTypes = db.ProgramTypes.OrderBy(t => t.priority);
            //for program details lightbox
            if (id != 0)
            {
                var program = db.Programs.Where(p => p.id == id && p.ProgramStatus.shown_to_student).SingleOrDefault();
                if (program != null)
                {
                    ViewBag.program_id = id;
                    programList.Add(program);
                }
                else
                {
                    Session["FlashMessage"] = "Program not found.";
                }
            }
            return View(programList);    
        }

        //
        // GET: /Program/Showall/5
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Showall(int id = 0)
        {
            ViewBag.typeid = id;

            ViewBag.programList = new SelectList(db.Programs.Where(p => p.ProgramStatus.shown_to_student).OrderBy(p => p.name), "id", "name");
            ViewBag.programTypeList = new SelectList(db.ProgramTypes, "id", "name", id);
            //ViewBag.programStatusList = new SelectList(db.ProgramStatus.Where(s => s.shown_to_student), "id", "name");

            StudentProfile student = db.StudentProfiles.Find(User.Identity.Name);
            if (student != null)
            {
                DateTime date_to_shown = new DateTime(DateTime.Now.Year - 2, 9, 1, 0, 0, 0);
                var programs = db.Programs.Include(p => p.ProgramStatus).Include(p => p.ProgramType).Where(p => (p.ProgramStatus.shown_to_student)
                    && ((id == 0) || (p.ProgramType.id == id))
                    && (p.application_end_time >= date_to_shown)
                );
                var programType = db.ProgramTypes.Find(id);
                if (programType != null)
                {
                    ViewBag.title = programType.name + " Programs";
                }
                else
                {
                    ViewBag.title = "All Programs";
                }
                List<ProgramAction> programactions = new List<ProgramAction>();
                foreach (var program in programs)
                {
                    ProgramAction programaction = new ProgramAction();
                    programaction.program = program;

                    //check student eligibility
                    programaction.eligible = IsEligible(program, student);
                    //check program application period
                    programaction.inperiod = (program.application_start_time <= DateTime.Now && DateTime.Now <= program.application_end_time);
                    //check if before program start time
                    programaction.beforestart = (DateTime.Now < program.application_start_time);
                    //check existing application
                    programaction.existed = student.Applications.Any(a => a.program_id == program.id);
                    if (programaction.existed)
                    {
                        //check existing application id
                        programaction.application = student.Applications.Where(a => a.program_id == program.id).SingleOrDefault();
                        //check application status
                        programaction.saved = programaction.application.ApplicationStatus.name == "Saved";
                    }
                    else
                    {
                        programaction.saved = false;
                    }
                    //check program status
                    programaction.open = program.ProgramStatus.name == "Opened";
                    programactions.Add(programaction);
                }
                return View(programactions.ToList());
            }
            else
            {
                List<ProgramAction> programactions = new List<ProgramAction>();
                return View(programactions.ToList());
            }
        }

        //
        // POST: /Program/Showall/5
        [HttpPost]
        [Authorize(Roles = "StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Showall(FormCollection Form, bool eligible, bool saved, bool submitted, int id = 0)
        {
            ViewBag.typeid = id;
            StudentProfile student = db.StudentProfiles.Find(User.Identity.Name);
            if (student != null)
            {
                DateTime date_to_shown = new DateTime(DateTime.Now.Year - 2, 9, 1, 0, 0, 0);
                var programs = db.Programs.Include(p => p.ProgramStatus).Include(p => p.ProgramType).Where(p => (p.ProgramStatus.shown_to_student)
                    && ((id == 0) || (p.ProgramType.id == id))
                    && (p.application_end_time >= date_to_shown)
                );
                var programType = db.ProgramTypes.Find(id);
                if (programType != null)
                {
                    ViewBag.title = programType.name + " Programs";
                }
                else
                {
                    ViewBag.title = "All Programs";
                }
                List<ProgramAction> programactions = new List<ProgramAction>();
                foreach (var program in programs)
                {
                    ProgramAction programaction = new ProgramAction();
                    programaction.program = program;

                    //check student eligibility
                    programaction.eligible = IsEligible(program, student);
                    //check program application period
                    programaction.inperiod = (program.application_start_time <= DateTime.Now && DateTime.Now <= program.application_end_time);
                    //check if before program start time
                    programaction.beforestart = (DateTime.Now < program.application_start_time);
                    //check existing application
                    programaction.existed = student.Applications.Any(a => a.program_id == program.id);
                    if (programaction.existed)
                    {
                        //check existing application id
                        programaction.application = student.Applications.Where(a => a.program_id == program.id).SingleOrDefault();
                        //check application status
                        programaction.saved = programaction.application.ApplicationStatus.name == "Saved";
                    }
                    else
                    {
                        programaction.saved = false;
                    }
                    //check program status
                    programaction.open = program.ProgramStatus.name == "Opened";
                    programactions.Add(programaction);
                }
                var selectedtype = Form["program_type"]!=null?Form["program_type"].Split(','):null;
                ViewBag.programList = new SelectList(db.Programs.Where(p => p.ProgramStatus.shown_to_student).OrderBy(p => p.name), "id", "name", Form["program"]);
                ViewBag.programTypeList = new MultiSelectList(db.ProgramTypes, "id", "name", selectedtype);
                //ViewBag.programStatusList = new SelectList(db.ProgramStatus.Where(s => s.shown_to_student), "id", "name", Form["program_status"]);
                
                return View(programactions.ToList().Where(p => (true)
                    && (String.IsNullOrEmpty(Form["program"]) || p.program.id.ToString() == Form["program"])
                    && (String.IsNullOrEmpty(Form["program_type"]) || selectedtype.Contains(p.program.type_id.ToString()))
                    && (String.IsNullOrEmpty(Form["program_status"]) || p.program.status_id.ToString() == Form["program_status"])
                    && ((!eligible && !saved && !submitted)
                    ||
                        (eligible && p.eligible && p.inperiod && p.open)
                        || (saved && (p.application != null && p.application.ApplicationStatus.name == "Saved"))
                        || (submitted && (p.application != null && p.application.ApplicationStatus.name != "Saved"))
                    )
                    ));
            }
            else
            {
                List<ProgramAction> programactions = new List<ProgramAction>();
                return View(programactions.ToList());
            }
        }

        //
        // GET: /Program/Details/5
        [Ajax(true)]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult Details(int id = 0)
        {
            //get program
            Program program = db.Programs.Find(id);
            if (program == null)
            {
                Session["FlashMessage"] = "Program not found.";
                if (User.IsInRole("Advising"))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return RedirectToAction("Record");
                }
            }

            var student = db.StudentProfiles.Find(User.Identity.Name);

            //initialize Model
            ProgramAction programaction = new ProgramAction();
            programaction.program = program;

            if (student != null)
            {
                //check student eligibility
                programaction.eligible = IsEligible(program, student);
                //check program application period
                programaction.inperiod = (program.application_start_time <= DateTime.Now && DateTime.Now <= program.application_end_time);
                //check if before program start time
                programaction.beforestart = (DateTime.Now < program.application_start_time);
                //check existing application
                programaction.existed = student.Applications.Any(a => a.program_id == program.id);
                if (programaction.existed)
                {
                    //check existing application id
                    programaction.application = student.Applications.Where(a => a.program_id == program.id).SingleOrDefault();
                    //check application status
                    programaction.saved = programaction.application.ApplicationStatus.name == "Saved";
                }
                else
                {
                    programaction.saved = false;
                }
                //check program status
                programaction.open = program.ProgramStatus.name == "Opened";
            }
            else
            {
                programaction.eligible = false;
                programaction.inperiod = (program.application_start_time <= DateTime.Now && DateTime.Now <= program.application_end_time);
                programaction.beforestart = (DateTime.Now < program.application_start_time);
                programaction.existed = false;
                programaction.saved = false;
                programaction.open = program.ProgramStatus.name == "Opened";

            }
            return PartialView(programaction);
        }

        //
        // GET: /Program/Create
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create()
        {
            ProgramViewModel ViewModel = new ProgramViewModel();
            Program program = new Program();

            //default values for create
            program.application_start_time = DateTime.Now.Date;
            program.application_end_time = DateTime.Now.Date.AddDays(7).AddHours(23).AddMinutes(59);
            program.eligible_academic_career = "UGRD";
            program.eligible_academic_organization = "CHEM,LIFS,MATH,PHYS,SSCI";
            program.eligible_program_status = "AC,LA";
            GetEligibleLists(program);
            int draftedStatusId = 0;
            try
            {
                draftedStatusId = db.ProgramStatus.Where(m => m.name == "Drafted").FirstOrDefault().id;
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Drafted Status not found." + e.Message;
            }
            ViewBag.StatusList = new SelectList(db.ProgramStatus, "id", "name", draftedStatusId);
            ViewBag.TypeList = new SelectList(db.ProgramTypes, "id", "name");
            ViewBag.ActionList = GetActionList(program);

            ViewModel.attachments = new List<ProgramAttachment>();
            for (int i = 0; i < ViewModel.no_of_attachments; i++)
            {
                ViewModel.attachments.Add(new ProgramAttachment());
            }

            ViewModel.program = program;
            return View(ViewModel);
        }

        //
        // POST: /Program/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Create(ProgramViewModel ViewModel)
        {
            Program program = ViewModel.program;
            GetEligibleLists(program);
            ViewBag.StatusList = new SelectList(db.ProgramStatus, "id", "name", program.status_id);
            ViewBag.TypeList = new SelectList(db.ProgramTypes, "id", "name", program.type_id);
            ViewBag.ActionList = GetActionList(program);

            //check appointment required, session exist when published
            try
            {
                ProgramStatus PublishedStatus = db.ProgramStatus.Where(m => m.name == "Opened").FirstOrDefault();
                if (program.status_id == PublishedStatus.id)
                {
                    if (program.require_appointment)
                    {
                        ProgramStatus DraftedStatus = db.ProgramStatus.Where(m => m.name == "Drafted").FirstOrDefault();
                        program.status_id = DraftedStatus.id;
                        Session["FlashMessage"] = "Appointment timeslot(s) must be created before publishing. Program Status is saved as 'Drafted'.";
                    }
                    else
                    {
                        program.published = DateTime.Now;
                        program.published_by = User.Identity.Name;
                    }
                }
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Opened Status not found." + e.Message;
                return View(ViewModel);
            }

            //check external link to add http
            if (program.apply_action == "link" && !program.apply_link.StartsWith("http", true, null))
            {
                program.apply_link = "http://" + program.apply_link;
            }
            
            //fixme: get eligible criteria input data from request object
            program.eligible_academic_career = String.IsNullOrEmpty(Request["academic_career"]) ? "" : Request["academic_career"];
            program.eligible_academic_organization = String.IsNullOrEmpty(Request["academic_organization"]) ? "" : Request["academic_organization"];
            program.eligible_academic_plan = String.IsNullOrEmpty(Request["academic_plan"]) ? "" : Request["academic_plan"];
            program.eligible_program_status = String.IsNullOrEmpty(Request["program_status"]) ? "" : Request["program_status"];
            program.eligible_academic_level = String.IsNullOrEmpty(Request["academic_level"]) ? "" : Request["academic_level"];

            if (ModelState.IsValid)
            {
                program.created = DateTime.Now;
                program.created_by = User.Identity.Name;
                program.modified = DateTime.Now;
                program.modified_by = User.Identity.Name;
                db.Programs.Add(program);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create program." + e.Message;
                    return View(ViewModel);
                }

                foreach (ProgramAttachment attachment in ViewModel.attachments)
                {
                    if (!String.IsNullOrEmpty(attachment.filename))
                    {
                        var sourcePath = Server.MapPath("~/App_Data/" + attachment.filepath);
                        var sourceFilepath = Path.Combine(sourcePath, attachment.filename);
                        var destPath = Server.MapPath("~/App_Data/" + "Attachments/Program/" + program.id);
                        var destFilepath = Path.Combine(destPath, attachment.filename);
                        Directory.CreateDirectory(destPath);
                        System.IO.File.Move(sourceFilepath, destFilepath);

                        attachment.program_id = program.id;
                        attachment.filepath = "Attachments/Program/" + program.id;
                        db.ProgramAttachments.Add(attachment);
                    }
                }
                //clear temp files uploaded but not used
                if (Directory.Exists(Server.MapPath("~/App_Data/Temp/Program/" + User.Identity.Name)))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Temp/Program/" + User.Identity.Name));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }

                foreach (ProgramOptionValue option in ViewModel.options)
                {
                    if (!String.IsNullOrEmpty(option.name))
                    {
                        option.program_id = program.id;
                        db.ProgramOptionValues.Add(option);
                    }
                }

                foreach (ProgramApplicationAttachment app_attachment in ViewModel.app_attachments)
                {
                    if (!String.IsNullOrEmpty(app_attachment.name))
                    {
                        app_attachment.program_id = program.id;
                        db.ProgramApplicationAttachments.Add(app_attachment);
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to add options/attachment to program." + e.Message;
                    return View(ViewModel);
                }

                return RedirectToAction("Index");
            }

            return View(ViewModel);
        }

        //
        // POST: /Program/Copy

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Copy(int id = 0, string name = null)
        {
            Program program = db.Programs.Find(id);
            if (program == null)
            {
                Session["FlashMessage"] = "Program not found.";
                return RedirectToAction("Edit", new { id = program.id });
            }
            program.name = String.IsNullOrEmpty(name) ? program.name + " - Copy" : name;
            ProgramStatus status_draft = db.ProgramStatus.Where(m => m.name == "Drafted").FirstOrDefault();
            program.status_id = status_draft.id;
            program.created = DateTime.Now;
            program.modified = DateTime.Now;
            program.created_by = User.Identity.Name;
            program.modified_by = User.Identity.Name;
            db.Programs.Add(program);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to copy program." + e.Message;
                return RedirectToAction("Edit", new { id = program.id });
            }
            return RedirectToAction("Edit", new { id = program.id });
        }

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Publish(int id = 0)
        {
            Program program = db.Programs.Find(id);
            if (program == null)
            {
                Session["FlashMessage"] = "Program not found.";
                return RedirectToAction("Index");
            }

            if (program.require_appointment)
            {
                if (!program.AppointmentConcerns.Any(c => c.Appointments.Count() > 0))
                {
                    Session["FlashMessage"] = "Appointment timeslot(s) must be created before publishing. Program not modified.";
                    return RedirectToAction("Index");
                }
            }

            try
            {
                ProgramStatus PublishedStatus = db.ProgramStatus.Where(m => m.name == "Opened").FirstOrDefault();
                program.status_id = PublishedStatus.id;
                program.published = DateTime.Now;
                program.published_by = User.Identity.Name;
                program.modified = DateTime.Now;
                program.modified_by = User.Identity.Name;
                db.Entry(program).State = EntityState.Modified;
                db.SaveChanges();
                Session["FlashMessage"] = "Program published succesfully.";
            }
            catch (Exception e)
            {
                Session["FlashMessage"] += " Failed to update database.\n" + e.Message;
                return RedirectToAction("Index");
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Program/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            ProgramViewModel ViewModel = new ProgramViewModel();
            Program program = db.Programs.Find(id);
            ViewModel.program = program;

            GetEligibleLists(program);
            ViewBag.StatusList = new SelectList(db.ProgramStatus, "id", "name", program.status_id);
            ViewBag.TypeList = new SelectList(db.ProgramTypes, "id", "name", program.type_id);
            ViewBag.ActionList = GetActionList(program);

            if (program == null)
            {
                Session["FlashMessage"] = "Program not found";
                return View(ViewModel);
            }

            ViewModel.attachments = new List<ProgramAttachment>(program.ProgramAttachments);
            for (int i = program.ProgramAttachments.Count(); i < ViewModel.no_of_attachments; i++)
            {
                ViewModel.attachments.Add(new ProgramAttachment());
            }

            ViewModel.options = new List<ProgramOptionValue>(program.ProgramOptionValues);
            for (int i = program.ProgramOptionValues.Count(); i < ViewModel.no_of_options; i++)
            {
                ViewModel.options.Add(new ProgramOptionValue());
            }

            ViewModel.app_attachments = new List<ProgramApplicationAttachment>(program.ProgramApplicationAttachments);
            for (int i = program.ProgramApplicationAttachments.Count(); i < ViewModel.no_of_app_attachments; i++)
            {
                ViewModel.app_attachments.Add(new ProgramApplicationAttachment());
            }

            return View(ViewModel);
        }

        //
        // POST: /Program/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(ProgramViewModel ViewModel)
        {
            Program program = ViewModel.program;
            GetEligibleLists(program);
            ViewBag.StatusList = new SelectList(db.ProgramStatus, "id", "name", program.status_id);
            ViewBag.TypeList = new SelectList(db.ProgramTypes, "id", "name", program.type_id);
            ViewBag.ActionList = GetActionList(program);

            //check appointment required, session exist when published
            try
            {
                ProgramStatus PublishedStatus = db.ProgramStatus.Where(m => m.name == "Opened").FirstOrDefault();
                if (program.status_id == PublishedStatus.id)
                {
                    if (program.require_appointment)
                    {
                        if (!db.Appointments.Any(o => o.AppointmentConcerns.Any(c => c.program_id == program.id)))
                        {
                            ProgramStatus DraftedStatus = db.ProgramStatus.Where(m => m.name == "Drafted").FirstOrDefault();
                            program.status_id = DraftedStatus.id;
                            Session["FlashMessage"] = "Appointment timeslot(s) must be created before publishing. Program Status is saved as 'Drafted'.";
                        }
                    }
                    else
                    {
                        program.published = DateTime.Now;
                        program.published_by = User.Identity.Name;
                    }
                }
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Opened Status not found." + e.Message;
                return View(ViewModel);
            }

            //check external link to add http
            if (program.apply_action == "link" && !program.apply_link.StartsWith("http", true, null))
            {
                program.apply_link = "http://" + program.apply_link;
            }

            program.eligible_academic_career = String.IsNullOrEmpty(Request["academic_career"]) ? "" : Request["academic_career"];
            program.eligible_academic_organization = String.IsNullOrEmpty(Request["academic_organization"]) ? "" : Request["academic_organization"];
            program.eligible_academic_plan = String.IsNullOrEmpty(Request["academic_plan"]) ? "" : Request["academic_plan"];
            program.eligible_program_status = String.IsNullOrEmpty(Request["program_status"]) ? "" : Request["program_status"];
            program.eligible_academic_level = String.IsNullOrEmpty(Request["academic_level"]) ? "" : Request["academic_level"];

            if (ModelState.IsValid)
            {
                program.modified = DateTime.Now;
                program.modified_by = User.Identity.Name;
                try
                {
                    db.Entry(program).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to update program." + e.Message;
                }

                foreach (ProgramAttachment attachment in ViewModel.attachments)
                {
                    if (attachment.id != 0)
                    {
                        ProgramAttachment att = db.ProgramAttachments.Find(attachment.id);
                        if (String.IsNullOrEmpty(attachment.filename))
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

                            db.ProgramAttachments.Remove(att);
                        }
                        else
                        {
                            if (att.filename != attachment.filename || att.filepath != attachment.filepath)
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
                                var sourcePath = Server.MapPath("~/App_Data/" + attachment.filepath);
                                var sourceFilepath = Path.Combine(sourcePath, attachment.filename);
                                var destPath = Server.MapPath("~/App_Data/" + "Attachments/Program/" + program.id);
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
                                }
                                catch (Exception e)
                                {
                                    Session["FlashMessage"] = "Failed to move file." + e.Message;
                                }

                                attachment.program_id = program.id;
                                attachment.filepath = "Attachments/Program/" + program.id;
                            }
                            try
                            {
                                db.Entry(att).CurrentValues.SetValues(attachment);
                            }
                            catch (Exception e)
                            {
                                Session["FlashMessage"] = "Failed to update attachment object values." + e.Message;
                            }
                        }
                    }
                    else if (!String.IsNullOrEmpty(attachment.filename))
                    {
                        var sourcePath = Server.MapPath("~/App_Data/" + attachment.filepath);
                        var sourceFilepath = Path.Combine(sourcePath, attachment.filename);
                        var destPath = Server.MapPath("~/App_Data/" + "Attachments/Program/" + program.id);
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
                        }
                        catch (Exception e)
                        {
                            Session["FlashMessage"] = "Failed to move file." + e.Message;
                        }

                        attachment.program_id = program.id;
                        attachment.filepath = "Attachments/Program/" + program.id;
                        db.ProgramAttachments.Add(attachment);
                    }
                }

                //clear temp files uploaded but not used
                if (Directory.Exists(Server.MapPath("~/App_Data/Temp/Program/" + program.id)))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Temp/Program/" + program.id));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }

                foreach (ProgramOptionValue option in ViewModel.options)
                {
                    if (option.id != 0)
                    {
                        if (!String.IsNullOrEmpty(option.name))
                        {
                            db.Entry(option).State = EntityState.Modified;
                        }
                        else
                        {
                            ProgramOptionValue opt = db.ProgramOptionValues.Find(option.id);
                            db.ProgramOptionValues.Remove(opt);
                        }
                    }
                    else if (!String.IsNullOrEmpty(option.name))
                    {
                        option.program_id = program.id;
                        db.ProgramOptionValues.Add(option);
                    }
                }

                foreach (ProgramApplicationAttachment app_attachment in ViewModel.app_attachments)
                {
                    if (app_attachment.id != 0)
                    {
                        if (!String.IsNullOrEmpty(app_attachment.name))
                        {
                            db.Entry(app_attachment).State = EntityState.Modified;
                        }
                        else
                        {
                            ProgramApplicationAttachment att = db.ProgramApplicationAttachments.Find(app_attachment.id);
                            db.ProgramApplicationAttachments.Remove(att);
                        }
                    }
                    else if (!String.IsNullOrEmpty(app_attachment.name))
                    {
                        app_attachment.program_id = program.id;
                        db.ProgramApplicationAttachments.Add(app_attachment);
                    }
                }
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to add/edit options/attachments to program. <br/><br/>" + e.Message;
                }
                return RedirectToAction("Index");
            }

            return View(program);
        }

        //
        // GET: /Program/BatchUpdate

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult BatchUpdate(string items)
        {
            var i = items.Split('_');
            var programs = db.Programs.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            ViewBag.StatusList = new SelectList(db.ProgramStatus, "id", "name");
            ViewBag.items = items;
            return PartialView(programs.ToList());
        }

        //
        // POST: /Program/BatchUpdate
        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult BatchUpdate(string items, int status_id)
        {
            var i = items.Split('_');
            var programs = db.Programs.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
            ViewBag.StatusList = new SelectList(db.ProgramStatus, "id", "name");
            ViewBag.items = items;
            foreach (var program in programs)
            {
                program.status_id = status_id;
                ProgramStatus status_publish = db.ProgramStatus.Where(m => m.name == "Opened").FirstOrDefault();
                if (program.status_id == status_publish.id)
                {
                    program.published = DateTime.Now;
                    program.published_by = User.Identity.Name;
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


        //
        // GET: /Program/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0)
        {
            Program program = db.Programs.Find(id);
            if (program == null)
            {
                return HttpNotFound();
            }
            return View(program);
        }

        //
        // POST: /Program/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Program program = db.Programs.Find(id);
            //db.ProgramAttachments.Where(a => a.program_id == program.id).ToList().ForEach(x => db.ProgramAttachments.Remove(x));
            //db.ProgramOptionValues.Where(o => o.program_id == program.id).ToList().ForEach(x => db.ProgramOptionValues.Remove(x));
            //db.ProgramApplicationAttachments.Where(aa => aa.program_id == program.id).ToList().ForEach(x => db.ProgramApplicationAttachments.Remove(x));
            //db.Programs.Remove(program);
            int cancelledStatusId = db.ProgramStatus.FirstOrDefault(s => s.name == "Cancelled").id;
            program.status_id = cancelledStatusId;
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to change program to delete status." + e.Message;
            }
            return RedirectToAction("Index");
        }

        public void GetEligibleLists(Program program)
        {
            var query = (from s in db.StudentProfiles
                    select new SelectListItem
                    {
                        Text = s.academic_career,
                        Value = s.academic_career
                    }).Distinct();
            String[] selected;
            if (!String.IsNullOrEmpty(program.eligible_academic_career))
            {
                selected = program.eligible_academic_career.Split(',');
            }
            else
            {
                selected = null;
            }
            ViewBag.eligible_academic_career = new MultiSelectList(query, "Value", "Text", selected);

            query = (from s in db.StudentProfiles
                     select new SelectListItem
                     {
                         Text = s.academic_organization,
                         Value = s.academic_organization
                     }).Distinct();
            if (!String.IsNullOrEmpty(program.eligible_academic_organization))
            {
                selected = program.eligible_academic_organization.Split(',');
            }
            else
            {
                selected = null;
            }
            ViewBag.eligible_academic_organization = new MultiSelectList(query, "Value", "Text", selected);

            query = (from s in db.StudentProfiles
                     select new SelectListItem
                     {
                         Text = s.academic_plan_description,
                         Value = s.academic_plan_primary
                     }).Distinct();
            if (!String.IsNullOrEmpty(program.eligible_academic_plan))
            {
                selected = program.eligible_academic_plan.Split(',');
            }
            else
            {
                selected = null;
            }
            ViewBag.eligible_academic_plan = new MultiSelectList(query, "Value", "Text", selected);

            query = (from s in db.StudentProfiles
                     select new SelectListItem
                     {
                         Text = s.program_status_description,
                         Value = s.program_status
                     }).Distinct();
            if (!String.IsNullOrEmpty(program.eligible_program_status))
            {
                selected = program.eligible_program_status.Split(',');
            }
            else
            {
                selected = null;
            }
            ViewBag.eligible_program_status = new MultiSelectList(query, "Value", "Text", selected);

            query = (from s in db.StudentProfiles
                     select new SelectListItem
                     {
                         Text = s.academic_level,
                         Value = s.academic_level
                     }).Distinct();
            if (!String.IsNullOrEmpty(program.eligible_academic_level))
            {
                selected = program.eligible_academic_level.Split(',');
            }
            else
            {
                selected = null;
            }
            ViewBag.eligible_academic_level = new MultiSelectList(query, "Value", "Text", selected);
        }

        public JsonResult GetEligibleAcademicOrganization(string selected)
        {
            List<string> selectedList = new List<string>(selected.Split(','));
            var query = (from s in db.StudentProfiles
                         where selectedList.Contains(s.academic_career)
                         select new SelectListItem
                         {
                             Text = s.academic_organization,
                             Value = s.academic_organization
                         }).Distinct();
            SelectList list = new SelectList(query, "Value", "Text");
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEligibleAcademicPlan(string selected)
        {
            List<string> selectedList = new List<string>(selected.Split(','));
            var query = (from s in db.StudentProfiles
                         where selectedList.Contains(s.academic_organization)
                         select new SelectListItem
                         {
                             Text = s.academic_plan_description,
                             Value = s.academic_plan_primary
                         }).Distinct();
            SelectList list = new SelectList(query, "Value", "Text");
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEligibleProgramStatus(string selected)
        {
            List<string> selectedList = new List<string>(selected.Split(','));
            var query = (from s in db.StudentProfiles
                         where selectedList.Contains(s.academic_plan_primary)
                         select new SelectListItem
                         {
                             Text = s.program_status_description,
                             Value = s.program_status
                         }).Distinct();
            SelectList list = new SelectList(query, "Value", "Text");
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEligibleAcademicLevel(string selected)
        {
            List<string> selectedList = new List<string>(selected.Split(','));
            var query = (from s in db.StudentProfiles
                         where selectedList.Contains(s.academic_level)
                         select new SelectListItem
                         {
                             Text = s.academic_level,
                             Value = s.academic_level
                         }).Distinct();
            SelectList list = new SelectList(query, "Value", "Text");
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        protected SelectList GetActionList(Program program)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "Default", Value = "default", Selected = true });
            items.Add(new SelectListItem { Text = "External Link", Value = "link" });
            items.Add(new SelectListItem { Text = "Email", Value = "email" });
            return new SelectList(items, "Value", "Text", program.apply_action);
        }

        //
        // POST: /Application/ExportToExcel/5

        public ActionResult ExportExcel(int id = 0)
        {
            ApplicationExportViewModel ViewModel = new ApplicationExportViewModel();
            ViewModel.program = db.Programs.Find(id);
            ViewModel.particulartypes = db.StudentParticularTypes.ToList();
            ViewModel.experiencetypes = db.StudentExperienceTypes.ToList();

            string strHtml = RenderRazorViewToString("ExportExcel", ViewModel);
            strHtml = HttpUtility.HtmlDecode(strHtml);//Html解碼
            byte[] b = System.Text.Encoding.UTF8.GetBytes(strHtml);//字串轉byte陣列
            Response.Write("<meta http-equiv=Content-Type content=text/html;charset=utf-8>");
            return File(b, "application/vnd.ms-excel", ViewModel.program.name + " Applications Export " + String.Format("{0:yyyyMMddHHmm}", DateTime.Now) + ".xls");//輸出檔案給Client端

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

        //
        // GET: /Application/MakeNotification/5

        public ActionResult MakeNotification(int id = 0)
        {
            var program = db.Programs.Find(id);
            if (program == null)
            {
                Session["FlashMessage"] = "Program not found.";
                return RedirectToAction("Index");
            }
            var notification = CreateNotification("ProgramPublished", program);
            if (notification != null)
            {
                return RedirectToAction("Index", "Notification", new { notification_id = notification.id });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        //
        // GET: /Application/MakeReminder/5

        public ActionResult MakeReminder(int id = 0)
        {
            var program = db.Programs.Find(id);
            if (program == null)
            {
                Session["FlashMessage"] = "Program not found.";
                return RedirectToAction("Index");
            }
            var notification = CreateNotification("ProgramDeadlindReminder", program);
            if (notification != null)
            {
                return RedirectToAction("Index", "Notification", new { notification_id = notification.id });
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}