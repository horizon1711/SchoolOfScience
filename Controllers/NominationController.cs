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

namespace SchoolOfScience.Controllers
{
    public class NominationController : ControllerBase
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /Nomination/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index()
        {
            var nomination = db.Nominations;
            ViewBag.programs = db.Programs.Where(p => p.require_nomination && p.Nominations.Count() == 0);
            return View(nomination.ToList());
        }

        //
        // GET: /Nomination/MyNomination

        [Authorize(Roles = "Nominator")]
        public ActionResult MyNomination(bool history = false)
        {
            ViewBag.history = history;
            var nominationlist = db.NominationLists.ToList().Where(l => l.UserProfile.UserName == User.Identity.Name
                && l.Nomination.NominationStatus.shortlisted
                && (history || l.end_date.AddDays(1) > DateTime.Now)
                && (!history || l.end_date.AddDays(1) <= DateTime.Now)
                );
            return View(nominationlist);
        }

        //
        // GET: /Nomination/History

        [Authorize(Roles = "Nominator")]
        public ActionResult History()
        {
            var nominated_applications = db.NominationApplications.Where(a => a.nominated && a.NominationList.UserProfile.UserName == User.Identity.Name);
            return View(nominated_applications.ToList());
        }

        //
        // GET: /Nomination/ApplicationList/5

        [Authorize(Roles = "Nominator")]
        public ActionResult ApplicationList(int id = 0, bool fulldetails = false)
        {
            ViewBag.fulldetails = fulldetails;
            var list = db.NominationLists.Find(id);
            if (list == null || list.UserProfile.UserName != User.Identity.Name)
            {
                Session["FlashMessage"] = "Program Nomination not found.";
                return RedirectToAction("MyNomination");
            }
            return View(list);
        }

        //
        // POST: /Nomination/NominateApplication

        [HttpPost]
        [Authorize(Roles = "Nominator")]
        public ActionResult Nominate(NominationApplication nominated_application, string action = "save", bool fulldetails = false)
        {
            ViewBag.fulldetails = fulldetails;

            db.Entry(nominated_application).State = EntityState.Modified;

            var list = db.NominationLists.Find(nominated_application.nomination_list_id);
            var nominator = list.UserProfile.UserName;
            if (list == null)
            {
                Session["FlashMessage"] = "Program Nomination not found.";
                return RedirectToAction("MyNomination");
            }
            if (nominator != User.Identity.Name)
            {
                Session["FlashMessage"] = "Nominator not found.";
                return RedirectToAction("MyNomination");
            }
            if (DateTime.Now < list.start_date || DateTime.Now > list.end_date.AddDays(1))
            {
                Session["FlashMessage"] = "Current date not in nomination period.";
                return RedirectToAction("MyNomination");
            }
            if (!list.Nomination.NominationStatus.nominated && !list.Nomination.NominationStatus.shortlisted)
            {
                Session["FlashMessage"] = "Program not in nomination status";
                return RedirectToAction("MyNomination");
            }
             
            if (ModelState.IsValid)
            {
                if (action == "submit")
                {
                    if (list.quota != 0 && list.NominationApplications.Count(a => a.nominated) >= list.quota)
                    {
                        Session["FlashMessage"] = "Nomination quota exceeded.";
                        return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_list_id, fulldetails = fulldetails });
                    }

                    nominated_application.nominate_date = DateTime.Now;
                    nominated_application.nominated = true;

                    var status = db.NominationStatus.FirstOrDefault(s => s.nominated && s.shortlisted);
                    if (status != null)
                    {
                        list.Nomination.NominationStatus = status;
                    }
                }

                try
                {
                    db.SaveChanges();
                    if (action == "submit")
                    {
                        if (list.quota == 0)
                        {
                            Session["FlashMessage"] = "Nomination successful.";
                        }
                        else if (list.NominationApplications.Count(a => a.nominated) == list.quota)
                        {
                            nominated_application.Application = db.Applications.Find(nominated_application.application_id);
                            SendNotification(CreateNotification("ApplicationNominated", list));
                            Session["FlashMessage"] = "Nomination completed. <br/><br/>Quota remaining: 0";
                        }
                        else
                        {
                            Session["FlashMessage"] = "Nomination successful. <br/><br/>Quota remaining: " + (list.quota - list.NominationApplications.Count(a => a.nominated)).ToString();
                        }
                    }
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to nominate the application.<br/><br/>" + e.Message;
                }
            }
            
            return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_list_id, fulldetails = fulldetails });
        }

        //
        // GET: /Nomination/CancelNominateApplication/5

        [Authorize(Roles = "Nominator")]
        public ActionResult CancelNominateApplication(int id = 0, bool fulldetails = false)
        {
            ViewBag.fulldetails = fulldetails;
            var nominated_application = db.NominationApplications.Find(id);
            var list = nominated_application.NominationList;
            var nominator = list.UserProfile.UserName;

            if (nominated_application == null)
            {
                Session["FlashMessage"] = "Nominated application not found.";
                return RedirectToAction("MyNomination");
            }
            if (list == null)
            {
                Session["FlashMessage"] = "Program Nomination not found.";
                return RedirectToAction("MyNomination");
            }
            if (nominator != User.Identity.Name)
            {
                Session["FlashMessage"] = "Nominator not found.";
                return RedirectToAction("MyNomination");
            }
            if (DateTime.Now < list.start_date || DateTime.Now > list.end_date.AddDays(1))
            {
                Session["FlashMessage"] = "Current date not in nomination period.";
                return RedirectToAction("MyNomination");
            }
            if (!list.Nomination.NominationStatus.nominated && !list.Nomination.NominationStatus.shortlisted)
            {
                Session["FlashMessage"] = "Program not in nomination status";
                return RedirectToAction("MyNomination");
            }

            nominated_application.nominated = false;
            nominated_application.nominate_date = null;

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to cancel the nomination.<br/><br/>" + e.Message;
            }

            return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_list_id, fulldetails = fulldetails });
            
        }

        //
        // GET: /Nomination/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Details(int id = 0)
        {
            Nomination nomination = db.Nominations.Find(id);
            if (nomination == null)
            {
                Session["FlashMessage"] = "Nomination not found";
                return RedirectToAction("Index");
            }
            return View(nomination);
        }

        //
        // GET: /Nomination/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create(int programid = 0)
        {
            ViewBag.statusList = db.NominationStatus;

            var program = db.Programs.Find(programid);
            if (program == null || !program.require_nomination)
            {
                Session["FlashMessage"] = "Program not found or nomination not required.";
                return RedirectToAction("Index");
            }

            var status = db.NominationStatus.SingleOrDefault(x => x.default_status);
            if (status == null)
            {
                Session["FlashMessage"] = "Default status not single or not found. Please go to Configuration->Nomination->Edit Status to configure.";
                return RedirectToAction("Index");
            }

            Nomination nomination = new Nomination
            {
                status_id = status.id,
                NominationStatus = status,
                program_id = program.id,
                Program = program,
                start_date = DateTime.Now.Date,
                end_date = DateTime.Now.Date.AddMonths(1)
            };

            return View(nomination);
        }

        //
        // POST: /Nomination/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Nomination nomination, string applicationids = null)
        {
            ViewBag.statusList = db.NominationStatus;

            if (ModelState.IsValid)
            {
                var program = db.Programs.Find(nomination.program_id);
                if (program == null || !program.require_nomination)
                {
                    Session["FlashMessage"] = "Program not found or nomination not required.";
                    return RedirectToAction("Index");
                }
                nomination.Program = program;

                if (nomination.start_date > nomination.end_date)
                {
                    Session["FlashMessage"] = "End date must be equal to or greater than start date.";
                    return View(nomination);
                }

                db.Nominations.Add(nomination);

                try
                {
                    db.SaveChanges();
                }

                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create nomination record.<br/><br/>" + e.Message;
                    return View(nomination);
                }
            }
            return RedirectToAction("CreateList", new { nomination_id = nomination.id });
        }

        //
        // GET: /Nomination/CreateList/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult CreateList(int nomination_id = 0)
        {
            var nomination = db.Nominations.Find(nomination_id);
            if (nomination == null)
            {
                Session["FlashMessage"] = "Nomination not found";
                return RedirectToAction("Index");
            }

            NominationList list = new NominationList
            {
                nomination_id = nomination.id,
                Nomination = nomination,
                start_date = nomination.start_date,
                end_date = nomination.end_date,
                nomination_level = 1,
                remarks = nomination.Program.special_criteria
            };

            ViewBag.nominatorList = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "Nominator"));

            return View(list);
        }

        //
        // POST: /Nomination/CreateList

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult CreateList(NominationList list, string applicationids)
        {
            ViewBag.nominatorList = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "Nominator"));

            var nomination = db.Nominations.Find(list.nomination_id);
            if (nomination == null)
            {
                Session["FlashMessage"] = "Nomination not found.";
                return RedirectToAction("Index");
            }
            list.Nomination = nomination;

            if (list.start_date < nomination.start_date
                || list.start_date > nomination.end_date
                || list.end_date < nomination.start_date
                || list.end_date > nomination.end_date)
            {
                Session["FlashMessage"] = "Nomination list period out of range. Must be between " + String.Format("{0:yyyy-MM-dd}", nomination.start_date) + " and " + String.Format("{0:yyyy-MM-dd}", nomination.end_date) + ".";
                return View(list);
            }
            if (list.start_date > list.end_date)
            {
                Session["FlashMessage"] = "End date must be equal to or greater than start date.";
                return View(list);
            }

            if (applicationids != null)
            {
                var ids = applicationids.Split('_');
                var shortlisted_applications = db.Applications.Where(p => ids.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
                foreach (Application application in shortlisted_applications)
                {
                    list.NominationApplications.Add(new NominationApplication
                    {
                        Nomination = list.Nomination,
                        Application = application,
                        nominate_date = null
                    });
                }
            }

            if (ModelState.IsValid)
            {
                db.NominationLists.Add(list);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to add nomination list.<br/><br/>" + e.Message;
                }
            }
            return RedirectToAction("Details", new { id = nomination.id });
        }

        //
        // GET: /Nomination/Edit/5

        //[Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            ViewBag.statusList = db.NominationStatus;
            Nomination nomination = db.Nominations.Find(id);
            if (nomination == null)
            {
                Session["FlashMessage"] = "Nomination not found";
                return RedirectToAction("Index");
            }
            return View(nomination);
        }

        //
        // POST: /Nomination/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Nomination nomination, string applicationids = null)
        {
            ViewBag.statusList = db.NominationStatus;

            var program = db.Programs.Find(nomination.program_id);
            if (program == null || !program.require_nomination)
            {
                Session["FlashMessage"] = "Program not found or nomination not required.";
                return RedirectToAction("Index");
            }
            nomination.Program = program;

            if (nomination.start_date > nomination.end_date)
            {
                Session["FlashMessage"] = "End date must be equal to or greater than start date.";
                return View(nomination);
            }
            if (ModelState.IsValid)
            {
                db.Entry(nomination).State = EntityState.Modified;
                
                var lists = db.NominationLists.Where(l => l.nomination_id == nomination.id);
                foreach (var list in lists)
                {
                    if (list.start_date < nomination.start_date
                        || list.start_date > nomination.end_date
                        || list.end_date < nomination.start_date
                        || list.end_date > nomination.end_date)
                    {
                        Session["FlashMessage"] = "Nomination period out of range. Nomination lists period must be within nomination period.";
                        return View(nomination);
                    }
                }

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = nomination.id });
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to edit nomination.<br/><br/>" + e.Message;
                }
            }
            return View(nomination);
        }

        //
        // GET: /Nomination/EditList/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult EditList(int id = 0)
        {
            NominationList list = db.NominationLists.Find(id);
            if (list == null)
            {
                Session["FlashMessage"] = "Nomination list not found";
                return RedirectToAction("Index");
            }
            ViewBag.nominatorList = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "Nominator"));
            return View(list);
        }

        //
        // POST: /Nomination/EditList

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult EditList(NominationList model, string applicationids)
        {
            ViewBag.nominatorList = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "Nominator"));

            var list = db.NominationLists.Find(model.id);

            if (ModelState.IsValid)
            {
                db.Entry(list).CurrentValues.SetValues(model);

                if (list.start_date < list.Nomination.start_date
                    || list.start_date > list.Nomination.end_date
                    || list.end_date < list.Nomination.start_date
                    || list.end_date > list.Nomination.end_date)
                {
                    Session["FlashMessage"] = "Nomination list period out of range. Must be between " + String.Format("{0:yyyy-MM-dd}", list.Nomination.start_date) + " and " + String.Format("{0:yyyy-MM-dd}", list.Nomination.end_date) + ".";
                    return View(list);
                }
                if (list.start_date > list.end_date)
                {
                    Session["FlashMessage"] = "End date must be equal to or greater than start date.";
                    return View(list);
                }

                if (applicationids != null)
                {
                    var ids = applicationids.Split('_');
                    foreach (var application in list.Nomination.Program.Applications.Where(a => a.ApplicationStatus.submitted && !a.ApplicationStatus.rejected))
                    {
                        if (list.NominationApplications.Any(a => a.application_id == application.id) != ids.Contains(application.id.ToString()))
                        {
                            if (ids.Contains(application.id.ToString()))
                            {
                                list.NominationApplications.Add(new NominationApplication
                                {
                                    Nomination = list.Nomination,
                                    Application = application,
                                    nominate_date = null
                                });
                            }
                            else
                            {
                                db.NominationApplications.Remove(list.NominationApplications.FirstOrDefault(a => a.application_id == application.id));
                            }
                        }
                    }
                }
                else
                {
                    List<NominationApplication> applications = list.NominationApplications.ToList();
                    foreach (var item in applications)
                    {
                        db.NominationApplications.Remove(item);
                    }
                }




                //List<NominationApplication> applications =  list.NominationApplications.ToList();
                //foreach (var item in applications)
                //{
                //    db.NominationApplications.Remove(item);
                //}
                //if (applicationids != null)
                //{
                //    var ids = applicationids.Split('_');
                //    var shortlisted_applications = db.Applications.Where(p => ids.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
                //    foreach (Application application in shortlisted_applications)
                //    {
                //        list.NominationApplications.Add(new NominationApplication
                //        {
                //            Nomination = list.Nomination,
                //            Application = application,
                //            nominate_date = null
                //        });
                //    }
                //}

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = list.Nomination.id });
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to edit nomination list.<br/><br/>" + e.Message;
                }
            }

            return View(list);
        }
        
        //
        // GET: /Nomination/Notify/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Notify(int id = 0)
        {
            var list = db.NominationLists.Find(id);
            if (list == null)
            {
                Session["FlashMessage"] = "Nomination list not found";
                return RedirectToAction("Details", new { id = list.Nomination.id });
            }
            SendNotification(CreateNotification("NominationShortlisted", list));
            return RedirectToAction("Details", new { id = list.Nomination.id });
        }

        //
        // GET: /Nomination/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0)
        {
            Nomination nomination = db.Nominations.Find(id);
            if (nomination == null)
            {
                Session["FlashMessage"] = "Nomination not found";
                return RedirectToAction("Index");
            }

            List<NominationList> lists = nomination.NominationLists.ToList();
            foreach (var list in lists)
            {
                List<NominationApplication> applications = list.NominationApplications.ToList();
                foreach (var item in applications)
                {
                    db.NominationApplications.Remove(item);
                }
                db.NominationLists.Remove(list);
            }

            db.Nominations.Remove(nomination);

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to cancel nomination.<br/><br/>" + e.Message;
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Nomination/DeleteList/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult DeleteList(int id = 0)
        {
            NominationList list = db.NominationLists.Find(id);
            if (list == null)
            {
                Session["FlashMessage"] = "Nomination list not found";
                return RedirectToAction("Index");
            }

            var nominationid = list.Nomination.id;

            List<NominationApplication> applications = list.NominationApplications.ToList();
            foreach (var item in applications)
            {
                db.NominationApplications.Remove(item);
            }

            db.NominationLists.Remove(list);
            

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to cancel nomination list.<br/><br/>" + e.Message;
            }

            return RedirectToAction("Details", new { id = nominationid });
        }

        //
        // POST: /Nomination/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    Nomination nomination = db.Nominations.Find(id);
        //    foreach (Application application in nomination.Program.Applications)
        //    {
        //        if (application.shortlisted)
        //        {
        //            application.shortlisted = false;
        //            application.nominated = false;
        //            application.modified = DateTime.Now;
        //            application.modified_by = User.Identity.Name;
        //            db.Entry(application).State = EntityState.Modified;
        //        }
        //    }
        //    db.Nominations.Remove(nomination);
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