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
    public class NominationController : Controller
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
        public ActionResult MyNomination()
        {
            var nominationlevel = db.NominationLevels.Where(l => l.Nominators.Any( n => n.nominator_username == User.Identity.Name));
            return View(nominationlevel.ToList());
        }

        //
        // GET: /Nomination/Nominate/5

        [Authorize(Roles = "Nominator")]
        public ActionResult ApplicationList(int id = 0)
        {
            var level = db.NominationLevels.Find(id);
            if (level == null || !level.Nominators.Any(n => n.nominator_username == User.Identity.Name))
            {
                Session["FlashMessage"] = "Program Nomination not found.";
                return RedirectToAction("MyNomination");
            }
            return View(level);
        }

        //
        // POST: /Nomination/NominateApplication

        [HttpPost]
        [Authorize(Roles = "Nominator")]
        public ActionResult NominateApplication(NominationApplication nominated_application)
        {
            var level = db.NominationLevels.Find(nominated_application.nomination_level_id);
            var nominator = level.Nominators.FirstOrDefault(n => n.nominator_username == User.Identity.Name);
            if (level == null)
            {
                Session["FlashMessage"] = "Program Nomination not found.";
                return RedirectToAction("MyNomination");
            }
            if (nominator == null)
            {
                Session["FlashMessage"] = "Nominator not found.";
                return RedirectToAction("MyNomination");
            }
            if (DateTime.Now < level.start_date || DateTime.Now > level.end_date.AddDays(1))
            {
                Session["FlashMessage"] = "Current date not in nomination period.";
                return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_level_id });
            }
            if (!level.Nomination.NominationStatus.nominated)
            {
                Session["FlashMessage"] = "Program not in nomination status";
                return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_level_id });
            }

            if (ModelState.IsValid)
            {
                nominated_application.nominated_date = DateTime.Now;
                if (nominated_application.id == 0)
                {
                    if ((level.quota != 0 && level.NominationApplications.Count() >= level.quota)
                        || (nominator.quota != 0 && nominator.NominationApplications.Count() >= nominator.quota)
                    )
                    {
                        Session["FlashMessage"] = "Nomination quota exceeded.";
                        return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_level_id });
                    }
                    db.NominationApplications.Add(nominated_application);
                }
                else
                {
                    db.Entry(nominated_application).State = EntityState.Modified;
                }

                var status = db.NominationStatus.FirstOrDefault(s => s.nominated && !s.shortlisted);
                if (status != null)
                {
                    level.Nomination.NominationStatus = status;
                }

                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to nominate the application.<br/><br/>" + e.Message;
                }
            }
            return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_level_id });
        }

        //
        // GET: /Nomination/CancelNominateApplication/5

        [Authorize(Roles = "Nominator")]
        public ActionResult CancelNominateApplication(int id = 0)
        {
            var nominated_application = db.NominationApplications.Find(id);
            var level = nominated_application.NominationLevel;
            var nominator = level.Nominators.FirstOrDefault(n => n.nominator_username == User.Identity.Name);
            if (nominated_application == null)
            {
                Session["FlashMessage"] = "Nominated application not found.";
                return RedirectToAction("MyNomination");
            }
            if (level == null)
            {
                Session["FlashMessage"] = "Program Nomination not found.";
                return RedirectToAction("MyNomination");
            }
            if (nominator == null)
            {
                Session["FlashMessage"] = "Nominator not found.";
                return RedirectToAction("MyNomination");
            }
            if (DateTime.Now < level.start_date || DateTime.Now > level.end_date.AddDays(1))
            {
                Session["FlashMessage"] = "Current date not in nomination period.";
                return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_level_id });
            }
            if (!level.Nomination.NominationStatus.nominated)
            {
                Session["FlashMessage"] = "Program not in nomination status";
                return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_level_id });
            }

            db.NominationApplications.Remove(nominated_application);

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to cancel the nomination.<br/><br/>" + e.Message;
            }
            return RedirectToAction("ApplicationList", new { id = nominated_application.nomination_level_id });
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
            var program = db.Programs.Find(programid);
            if (program == null || !program.require_nomination)
            {
                Session["FlashMessage"] = "Program not found or nomination not required.";
                return RedirectToAction("Index");
            }

            var status = db.NominationStatus.FirstOrDefault(x => x.shortlisted);
            if (status == null)
            {
                Session["FlashMessage"] = "Shortlisted status not found";
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

            ViewBag.statusList = new SelectList(db.NominationStatus, "id", "name");
            return View(nomination);
        }

        //
        // POST: /Nomination/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Nomination nomination, string applicationids = null)
        {
            if (ModelState.IsValid)
            {
                ViewBag.statusList = new SelectList(db.NominationStatus, "id", "name");
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
                NominationLevel level = new NominationLevel
                {
                    nomination_level = 0,//level 0 represents shortlisted by mySCI admin staff
                    start_date = nomination.start_date,
                    end_date = nomination.end_date,
                    quota = 0//default no limit for shortlisted applications
                };
                nomination.NominationLevels.Add(level);

                var nominator_user = db.SystemUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                if (nominator_user == null)
                {
                    Session["FlashMessage"] = "User not found";
                    return RedirectToAction("Index");
                }
                Nominator nominator = new Nominator
                {
                    NominationLevel = level,
                    nominator_username = nominator_user.UserName,
                    quota = 0//default no limit for shortlisted applications
                };
                level.Nominators.Add(nominator);

                if (!String.IsNullOrEmpty(applicationids))
                {
                    var i = applicationids.Split('_');
                    var shortlisted_applications = db.Applications.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
                    if (shortlisted_applications != null)
                    {
                        foreach (Application application in shortlisted_applications)
                        {
                            level.NominationApplications.Add(new NominationApplication
                            {
                                Application = application,
                                Nominator = nominator,
                                NominationLevel = level,
                                nominated_date = DateTime.Now,
                            });
                        }
                    }
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
            return RedirectToAction("AddLevel", new { id = nomination.id });
        }

        //
        // GET: /Nomination/AddLevel/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult AddLevel(int id = 0)
        {
            NominationLevel level = new NominationLevel();
            var nomination = db.Nominations.Find(id);
            if (nomination == null || !nomination.NominationLevels.Any(l => l.nomination_level == 0))
            {
                Session["FlashMessage"] = "Nomination not found";
                return RedirectToAction("Index");
            }
            level.nomination_id = nomination.id;
            level.Nomination = nomination;
            level.start_date = nomination.start_date;
            level.end_date = nomination.end_date;
            level.quota = 0;
            level.nomination_level = nomination.NominationLevels.Max(l => l.nomination_level) + 1;

            ViewBag.nominatorList = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "Nominator"));

            return View(level);
        }

        //
        // POST: /Nomination/AddLevel

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult AddLevel(NominationLevel level, int[] nominatorids)
        {
            ViewBag.nominatorList = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "Nominator"));
            var nomination = db.Nominations.Find(level.nomination_id);
            if (nomination == null)
            {
                Session["FlashMessage"] = "Nomination not found.";
                return RedirectToAction("Index");
            }
            level.Nomination = nomination;

            if (nominatorids != null)
            {
                var nominator_users = db.SystemUsers.Where(u => nominatorids.Contains(u.UserId));
                foreach (var nominator_user in nominator_users)
                {
                    level.Nominators.Add(new Nominator
                    {
                        NominationLevel = level,
                        nominator_username = nominator_user.UserName,
                        quota = level.quota
                    });
                }
            }
            else
            {
                Session["FlashMessage"] = "Nominators must not be empty.";
                return View(level);
            }
            if (level.start_date < nomination.start_date
                || level.start_date > nomination.end_date
                || level.end_date < nomination.start_date
                || level.end_date > nomination.end_date)
            {
                Session["FlashMessage"] = "Nomination level period out of range. Must be between " + String.Format("{0:yyyy-MM-dd}", nomination.start_date) + " and " + String.Format("{0:yyyy-MM-dd}", nomination.end_date) + ".";
                return View(level);
            }
            if (level.start_date > level.end_date)
            {
                Session["FlashMessage"] = "End date must be equal to or greater than start date.";
                return View(level);
            }
            if (ModelState.IsValid)
            {
                db.NominationLevels.Add(level);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to add nomination level.<br/><br/>" + e.Message;
                }
            }
            return RedirectToAction("Details", new { id = nomination.id });
        }

        //
        // GET: /Nomination/Edit/5

        //[Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            Nomination nomination = db.Nominations.Find(id);
            if (nomination == null)
            {
                Session["FlashMessage"] = "Nomination not found";
                return RedirectToAction("Index");
            }
            ViewBag.statusList = new SelectList(db.NominationStatus, "id", "name");
            return View(nomination);
        }

        //
        // POST: /Nomination/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Nomination nomination, string applicationids = null)
        {
            ViewBag.statusList = new SelectList(db.NominationStatus, "id", "name");
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

                var level = db.NominationLevels.FirstOrDefault(l => l.nomination_id == nomination.id && l.nomination_level == 0);
                if (level == null)
                {
                    level = new NominationLevel
                    {
                        nomination_level = 0,//level 0 represents shortlisted by mySCI admin staff
                        start_date = nomination.start_date,
                        end_date = nomination.end_date
                    };
                    nomination.NominationLevels.Add(level);
                }
                else
                {
                    level.start_date = nomination.start_date;
                    level.end_date = nomination.end_date;
                }


                var otherlevels = db.NominationLevels.Where(l => l.nomination_id == nomination.id && l.nomination_level > 0);
                foreach (var otherlevel in otherlevels)
                {
                    if (otherlevel.start_date < nomination.start_date
                        || otherlevel.start_date > nomination.end_date
                        || otherlevel.end_date < nomination.start_date
                        || otherlevel.end_date > nomination.end_date)
                    {
                        Session["FlashMessage"] = "Nomination period out of range. Related nomination levels period must be between nomination period. (Error: Nomination Level " + otherlevel.nomination_level.ToString() + " " + String.Format("{0:yyyy-MM-dd}", otherlevel.start_date) + " to " + String.Format("{0:yyyy-MM-dd}", otherlevel.end_date) + ")";
                        nomination.NominationLevels.Add(level);
                        return View(nomination);
                    }
                }

                var nominator = level.Nominators.FirstOrDefault(n => n.nominator_username == User.Identity.Name);
                if (nominator == null)
                {
                    var nominator_user = db.SystemUsers.FirstOrDefault(u => u.UserName == User.Identity.Name);
                    if (nominator_user == null)
                    {
                        Session["FlashMessage"] = "User not found";
                        return RedirectToAction("Index");
                    }
                    nominator = new Nominator
                    {
                        NominationLevel = level,
                        nominator_username = nominator_user.UserName,
                    };
                    level.Nominators.Add(nominator);
                }

                var nominated_applications = db.NominationApplications.Where(a => a.nomination_level_id == level.id);
                foreach (var nominated_application in nominated_applications)
                {
                    db.NominationApplications.Remove(nominated_application);
                }
                if (!String.IsNullOrEmpty(applicationids))
                {
                    var i = applicationids.Split('_');
                    var shortlisted_applications = db.Applications.Where(p => i.Contains(SqlFunctions.StringConvert((double)p.id).Trim()));
                    if (shortlisted_applications != null)
                    {
                        foreach (Application application in shortlisted_applications)
                        {
                            level.NominationApplications.Add(new NominationApplication
                            {
                                Application = application,
                                Nominator = nominator,
                                NominationLevel = level,
                                nominated_date = DateTime.Now,
                            });
                        }
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
        // GET: /Nomination/EditLevel/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult EditLevel(int id = 0)
        {
            NominationLevel level = db.NominationLevels.Find(id);
            if (level == null)
            {
                Session["FlashMessage"] = "Nomination level not found";
                return RedirectToAction("Index");
            }
            ViewBag.nominatorList = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "Nominator"));
            ViewBag.selectedNominators = db.SystemUsers.ToList().Where(u => level.Nominators.Any(n => n.nominator_username == u.UserName)).Select(n => n.UserId);
            return View(level);
        }

        //
        // POST: /Nomination/EditLevel

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult EditLevel(NominationLevel level, int[] nominatorids)
        {
            ViewBag.nominatorList = db.SystemUsers.Where(u => u.UserRoles.Any(r => r.RoleName == "Nominator"));
            if (ModelState.IsValid)
            {
                db.Entry(level).State = EntityState.Modified;

                if (nominatorids != null)
                {
                    var nominator_users = db.SystemUsers.Where(u => nominatorids.Contains(u.UserId));
                    var nominators = db.Nominators.Where(n => n.nomination_level_id == level.id);

                    foreach (var nominator in nominators)
                    {
                        nominator.quota = level.quota;
                    }

                    foreach (var nominator in nominators.Where(n => n.NominationApplications.Count() == 0 && !nominator_users.Any(u => u.UserName == n.nominator_username)))
                    {
                        db.Nominators.Remove(nominator);
                    }

                    foreach (var nominator_user in nominator_users)
                    {
                        if (!nominators.Any(n => n.nominator_username == nominator_user.UserName))
                        {
                            level.Nominators.Add(new Nominator
                            {
                                NominationLevel = level,
                                nominator_username = nominator_user.UserName,
                                quota = level.quota
                            });
                        }
                    }
                }
                else
                {
                    Session["FlashMessage"] = "Nominators must not be empty.";
                    return View(level);
                }

                var nomination = db.Nominations.Find(level.nomination_id);
                if (nomination == null)
                {
                    Session["FlashMessage"] = "Nomination not found.";
                    return RedirectToAction("Index");
                }
                if (level.start_date < nomination.start_date
                    || level.start_date > nomination.end_date
                    || level.end_date < nomination.start_date
                    || level.end_date > nomination.end_date)
                {
                    Session["FlashMessage"] = "Nomination level period out of range. Must be between " + String.Format("{0:yyyy-MM-dd}", nomination.start_date) + " and " + String.Format("{0:yyyy-MM-dd}", nomination.end_date) + ".";
                    return View(level);
                }
                if (level.start_date > level.end_date)
                {
                    Session["FlashMessage"] = "End date must be equal to or greater than start date.";
                    return View(level);
                }

                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Details", new { id = nomination.id });
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to edit nomination level.<br/><br/>" + e.Message;
                }
            }

            return View(level);
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

            List<NominationLevel> levels = nomination.NominationLevels.ToList();
            foreach (var level in levels)
            {
                List<NominationApplication> applications = level.NominationApplications.ToList();
                foreach (var item in applications)
                {
                    db.NominationApplications.Remove(item);
                }
                List<Nominator> nominators = level.Nominators.ToList();
                foreach (var item in nominators)
                {
                    db.Nominators.Remove(item);
                }
                db.NominationLevels.Remove(level);
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
        // GET: /Nomination/DeleteLevel/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult DeleteLevel(int id = 0)
        {
            NominationLevel level = db.NominationLevels.Find(id);
            if (level == null)
            {
                Session["FlashMessage"] = "Nomination level not found";
                return RedirectToAction("Index");
            }

            var nominationid = level.Nomination.id;

            foreach (var deletelevel in db.NominationLevels.Where(l => l.nomination_level >= level.nomination_level))
            {
                List<NominationApplication> applications = deletelevel.NominationApplications.ToList();
                foreach (var item in applications)
                {
                    db.NominationApplications.Remove(item);
                }
                List<Nominator> nominators = deletelevel.Nominators.ToList();
                foreach (var item in nominators)
                {
                    db.Nominators.Remove(item);
                }
                db.NominationLevels.Remove(deletelevel);
            }

            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to cancel nomination level.<br/><br/>" + e.Message;
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