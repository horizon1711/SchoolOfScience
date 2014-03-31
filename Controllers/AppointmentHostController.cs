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

namespace SchoolOfScience.Controllers
{
    [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
    public class AppointmentHostController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /AppointmentHost/

        public ActionResult Index()
        {
            return View(db.AppointmentHosts.ToList());
        }

        //
        // GET: /AppointmentHost/Details/5

        public ActionResult Details(int id = 0)
        {
            AppointmentHost appointmenthost = db.AppointmentHosts.Find(id);
            if (appointmenthost == null)
            {
                Session["FlashMessage"] = "Appointment Host not found.";
                return RedirectToAction("Index");
            }
            return View(appointmenthost);
        }

        //
        // GET: /AppointmentHost/Create

        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /AppointmentHost/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AppointmentHost appointmenthost)
        {
            if (ModelState.IsValid)
            {
                db.AppointmentHosts.Add(appointmenthost);
                db.SaveChanges();
                return RedirectToAction("Edit", new { id = appointmenthost.id });
            }

            return View(appointmenthost);
        }

        //
        // GET: /AppointmentHost/Edit/5

        public ActionResult Edit(int id = 0)
        {
            AppointmentHost appointmenthost = db.AppointmentHosts.Find(id);
            ViewBag.userList = new MultiSelectList(db.SystemUsers.Where(u => !u.UserRoles.Any(r => r.RoleName.StartsWith("Student"))), "UserId", "UserName", appointmenthost.SystemUsers.Select(u => u.UserId.ToString()));
            if (appointmenthost == null)
            {
                Session["FlashMessage"] = "Appointment Host not found.";
                return RedirectToAction("Index");
            }
            AppointmentHostViewModel ViewModel = new AppointmentHostViewModel();
            ViewModel.host = appointmenthost;
            return View(ViewModel);
        }

        //
        // POST: /AppointmentHost/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentHostViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                AppointmentHost appointmenthost = db.AppointmentHosts.Find(ViewModel.host.id);
                db.Entry(appointmenthost).CurrentValues.SetValues(ViewModel.host);
                appointmenthost.SystemUsers.Clear();
                if (ViewModel.userids != null)
                {
                    var users = db.SystemUsers.Where(u => ViewModel.userids.Contains(u.UserId));
                    foreach (var user in users)
                    {
                        appointmenthost.SystemUsers.Add(user);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(ViewModel);
        }

        //
        // GET: /AppointmentHost/Delete/5

        public ActionResult Delete(int id = 0)
        {
            AppointmentHost appointmenthost = db.AppointmentHosts.Find(id);
            if (appointmenthost == null)
            {
                Session["FlashMessage"] = "Appointment Host not found.";
                return RedirectToAction("Index");
            }
            if (appointmenthost.Appointments != null && appointmenthost.Appointments.Count() > 0)
            {
                Session["FlashMessage"] = "Appointment Host is attached to existing Appointment(s).";
                return RedirectToAction("Index");
            }
            return View(appointmenthost);
        }

        //
        // POST: /AppointmentHost/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AppointmentHost appointmenthost = db.AppointmentHosts.Find(id);
            appointmenthost.SystemUsers.Clear();
            db.AppointmentHosts.Remove(appointmenthost);
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