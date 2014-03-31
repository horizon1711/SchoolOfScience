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
    [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
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
                Session["FlashMessage"] = "User Profile not found.";
                return RedirectToAction("Index");
            }

            ViewModel.profile = userprofile;
            ViewBag.roleList = new MultiSelectList(db.UserRoles, "RoleId", "RoleName", userprofile.UserRoles.Select(r => r.RoleId.ToString()));
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
                db.Entry(userprofile).CurrentValues.SetValues(ViewModel.profile);
                userprofile.UserRoles.Clear();
                if (ViewModel.roleids != null)
                {
                    var roles = db.UserRoles.Where(r => ViewModel.roleids.Contains(r.RoleId));
                    foreach (var role in roles)
                    {
                        userprofile.UserRoles.Add(role);
                    }
                }

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