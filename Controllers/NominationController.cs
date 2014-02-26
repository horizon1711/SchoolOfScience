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

namespace SchoolOfScience.Controllers
{
    public class NominationController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /Nomination/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment, Nominator")]
        public ActionResult Index()
        {
            var nomination = db.Nominations.Where(n => n.nominator_id == WebSecurity.CurrentUserId).Include(n => n.NominationStatus).Include(n => n.Program);
            if (User.IsInRole("Advising"))
            {
                nomination = db.Nominations.Include(n => n.NominationStatus).Include(n => n.Program);
                ViewBag.programs = db.Programs.Where(p => p.require_nomination && !nomination.Any(n => n.program_id == p.id));
            }
            return View(nomination.ToList());
        }
        //
        // GET: /Nomination/Nominate

        [Authorize(Roles = "Admin,Advising,StudentDevelopment, Nominator")]
        public ActionResult Nominate(int id = 0)
        {
            NominationViewModel ViewModel = new NominationViewModel();
            Nomination nomination = db.Nominations.Find(id);
            if (nomination == null)
            {
                return HttpNotFound("Nomination not found");
            }
            List<Application> applications = nomination.Program.Applications.Where(a => a.shortlisted).ToList();

            ViewModel.nomination = nomination;
            ViewModel.applications = applications;
            return View(ViewModel);
        }

        //
        // GET: /Nomination/Nominate

        [HttpPost]
        [Authorize(Roles = "Nominator")]
        public ActionResult Nominate(NominationViewModel ViewModel)
        {
            int nominatedStatusId = db.NominationStatus.Where(s => s.name == "Nominated").FirstOrDefault().id;
            Nomination nomination = ViewModel.nomination;
            nomination.status_id = nominatedStatusId;
            db.Entry(nomination).State = EntityState.Modified;

            foreach (Application application in ViewModel.applications)
            {
                var original = db.Applications.Find(application.id);
                if (application.nominated != original.nominated)
                {
                    original.modified = DateTime.Now;
                    original.modified_by = User.Identity.Name;
                    original.nominated = application.nominated;
                    db.Entry(original).State = EntityState.Modified;
                }
            }

            db.SaveChanges();
            return RedirectToAction("Nominate", new { id = ViewModel.nomination.id });
        }

        //
        // GET: /Nomination/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Details(int id = 0)
        {
            Nomination nomination = db.Nominations.Find(id);
            if (nomination == null)
            {
                return HttpNotFound();
            }
            return View(nomination);
        }

        //
        // GET: /Nomination/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create(int id = 0)
        {
            NominationViewModel ViewModel = new NominationViewModel();
            ViewModel.nomination = new Nomination();
            ViewModel.applications = new List<Application>();

            Program program = db.Programs.Where(p => p.id == id && p.require_nomination).FirstOrDefault();
            if (program == null)
            {
                return HttpNotFound("Program not found");
            }
            List<Application> applications = db.Applications.Where(a => a.program_id == program.id).ToList();
            if (applications == null)
            {
                return HttpNotFound("Application not found");
            }


            //fix me: user profile relationship
            List<SelectListItem> nominators = new List<SelectListItem>();
            nominators.Add(new SelectListItem{
                Value = "7",
                Text = "nominator"
            });

            int openedStatusId = db.NominationStatus.Where(x => x.name == "Opened").FirstOrDefault().id;
            ViewBag.statusList = new SelectList(db.NominationStatus, "id", "name");
            ViewBag.nominatorList = new SelectList(nominators, "Value", "Text");
            ViewModel.nomination.program_id = program.id;
            ViewModel.nomination.Program = program;
            ViewModel.applications = applications;
            ViewModel.nomination.nominator_id = 7;
            ViewModel.nomination.status_id = openedStatusId;
            ViewModel.nomination.start_date = DateTime.Now.Date;
            ViewModel.nomination.end_date = DateTime.Now.Date;
            return View(ViewModel);
        }

        //
        // POST: /Nomination/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NominationViewModel ViewModel)
        {
            Nomination nomination = ViewModel.nomination;
            if (ModelState.IsValid)
            {
                db.Nominations.Add(nomination);

                foreach (Application application in ViewModel.applications)
                {
                    var original = db.Applications.Find(application.id);
                    if (application.shortlisted != original.shortlisted)
                    {
                        original.modified = DateTime.Now;
                        original.modified_by = User.Identity.Name;
                        original.shortlisted = application.shortlisted;
                        db.Entry(original).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.status_id = new SelectList(db.NominationStatus, "id", "name", nomination.status_id);
            ViewBag.program_id = new SelectList(db.Programs, "id", "name", nomination.program_id);
            return View(ViewModel);
        }

        //
        // GET: /Nomination/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            NominationViewModel ViewModel = new NominationViewModel();
            ViewModel.nomination = new Nomination();
            ViewModel.applications = new List<Application>();
            Nomination nomination = db.Nominations.Find(id);
            if (nomination == null)
            {
                return HttpNotFound();
            }

            Program program = db.Programs.Where(p => p.id == nomination.program_id && p.require_nomination).FirstOrDefault();
            if (program == null)
            {
                return HttpNotFound("Program not found");
            }
            List<Application> applications = db.Applications.Where(a => a.program_id == program.id).ToList();
            if (applications == null)
            {
                return HttpNotFound("Application not found");
            }
            ViewModel.nomination = nomination;
            ViewModel.applications = applications;


            //fix me: user profile relationship
            List<SelectListItem> nominators = new List<SelectListItem>();
            nominators.Add(new SelectListItem
            {
                Value = "7",
                Text = "nominator"
            });

            int openedStatusId = db.NominationStatus.Where(x => x.name == "Opened").FirstOrDefault().id;
            ViewBag.statusList = new SelectList(db.NominationStatus, "id", "name");
            ViewBag.nominatorList = new SelectList(nominators, "Value", "Text");
            ViewModel.nomination = nomination;
            return View(ViewModel);
        }

        //
        // POST: /Nomination/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(NominationViewModel ViewModel)
        {
            Nomination nomination = ViewModel.nomination;
            if (ModelState.IsValid)
            {
                db.Entry(nomination).State = EntityState.Modified;
                db.SaveChanges();

                foreach (Application application in ViewModel.applications)
                {
                    var original = db.Applications.Find(application.id);
                    if (application.shortlisted != original.shortlisted)
                    {
                        original.modified = DateTime.Now;
                        original.modified_by = User.Identity.Name;
                        original.shortlisted = application.shortlisted;
                        db.Entry(original).State = EntityState.Modified;
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            //fix me: user profile relationship
            List<SelectListItem> nominators = new List<SelectListItem>();
            nominators.Add(new SelectListItem
            {
                Value = "7",
                Text = "nominator"
            });
            ViewBag.statusList = new SelectList(db.NominationStatus, "id", "name");
            ViewBag.nominatorList = new SelectList(nominators, "Value", "Text");
            return View(ViewModel);
        }

        //
        // GET: /Nomination/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0)
        {
            Nomination nomination = db.Nominations.Find(id);
            if (nomination == null)
            {
                return HttpNotFound();
            }
            return View(nomination);
        }

        //
        // POST: /Nomination/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Nomination nomination = db.Nominations.Find(id);
            foreach (Application application in nomination.Program.Applications)
            {
                if (application.shortlisted)
                {
                    application.shortlisted = false;
                    application.nominated = false;
                    application.modified = DateTime.Now;
                    application.modified_by = User.Identity.Name;
                    db.Entry(application).State = EntityState.Modified;
                }
            }
            db.Nominations.Remove(nomination);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}