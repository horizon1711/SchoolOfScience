using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using SchoolOfScience.Models.ViewModels;

namespace SchoolOfScience.Controllers
{
    public class UserProfileController : Controller
    {
        private UsersContext db = new UsersContext();

        //
        // GET: /UserProfile/

        public ActionResult Index()
        {
            return View(db.UserProfiles.ToList());
        }

        //
        // GET: /UserProfile/Details/5

        public ActionResult Details(int id = 0)
        {
            UserProfile userprofile = db.UserProfiles.Find(id);
            if (userprofile == null)
            {
                return HttpNotFound();
            }
            return View(userprofile);
        }

        ////
        //// GET: /UserProfile/Create

        //public ActionResult Create()
        //{
        //    return View();
        //}

        ////
        //// POST: /UserProfile/Create

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(UserProfile userprofile)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.UserProfiles.Add(userprofile);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(userprofile);
        //}

        //
        // GET: /UserProfile/Edit/5

        public ActionResult Edit(int id = 0)
        {
            UserProfileViewModel ViewModel = new UserProfileViewModel();
            var userprofile = db.UserProfiles.Find(id);

            if (userprofile == null)
            {
                return HttpNotFound("User Profile not found.");
            }

            ViewModel.profile = userprofile;
            if (userprofile.UserRoles.Count == 1)
            {
                ViewModel.role_id = userprofile.UserRoles.SingleOrDefault().RoleId;
            }
            ViewBag.roleList = new SelectList(db.UserRoles, "RoleId", "RoleName");
            return View(ViewModel);
        }

        //
        // POST: /UserProfile/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UserProfileViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                UserProfile userprofile = db.UserProfiles.Find(ViewModel.profile.UserId);
                userprofile.UserRoles.Clear();
                userprofile.UserRoles.Add(db.UserRoles.Find(ViewModel.role_id));
                db.Entry(userprofile).CurrentValues.SetValues(ViewModel.profile);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.roleList = new SelectList(db.UserRoles, "RoleId", "RoleName");
            return View(ViewModel);
        }

        //
        // GET: /UserProfile/Delete/5

        //public ActionResult Delete(int id = 0)
        //{
        //    UserProfile userprofile = db.UserProfiles.Find(id);
        //    if (userprofile == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userprofile);
        //}

        ////
        //// POST: /UserProfile/Delete/5

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(int id)
        //{
        //    UserProfile userprofile = db.UserProfiles.Find(id);
        //    db.UserProfiles.Remove(userprofile);
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