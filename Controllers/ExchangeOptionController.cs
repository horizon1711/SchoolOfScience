using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;

namespace SchoolOfScience.Controllers
{
    public class ExchangeOptionController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /ExchangeOption/

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Index()
        {
            return View(db.ExchangeOptions.ToList());
        }
        //
        // GET: /ExchangeOption/List

        [Authorize(Roles = "Admin,Advising,StudentDevelopment,StudentUGRD,StudentRPGTPG,StudentNUGD")]
        public ActionResult List()
        {
            return PartialView(db.ExchangeOptions.Where(e => e.status).OrderBy(e => e.name).ToList());
        }

        //
        // GET: /ExchangeOption/Details/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Details(int id = 0)
        {
            ExchangeOption exchangeoption = db.ExchangeOptions.Find(id);
            if (exchangeoption == null)
            {
                return HttpNotFound();
            }
            return View(exchangeoption);
        }

        //
        // GET: /ExchangeOption/Create

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Create()
        {
            ExchangeOption m = new ExchangeOption();
            m.status = true;
            return View(m);
        }

        //
        // POST: /ExchangeOption/Create

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ExchangeOption exchangeoption)
        {
            if (ModelState.IsValid)
            {
                db.ExchangeOptions.Add(exchangeoption);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create exchange destination." + e.Message;
                    return View(exchangeoption);
                }
                return RedirectToAction("Index");
            }

            return View(exchangeoption);
        }

        //
        // GET: /ExchangeOption/Edit/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Edit(int id = 0)
        {
            ExchangeOption exchangeoption = db.ExchangeOptions.Find(id);
            if (exchangeoption == null)
            {
                return HttpNotFound();
            }
            return View(exchangeoption);
        }

        //
        // POST: /ExchangeOption/Edit/5

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ExchangeOption exchangeoption)
        {
            if (ModelState.IsValid)
            {
                db.Entry(exchangeoption).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to edit exchange destination." + e.Message;
                    return View(exchangeoption);
                }
                return RedirectToAction("Index");
            }
            return View(exchangeoption);
        }

        //
        // GET: /ExchangeOption/Delete/5

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Delete(int id = 0)
        {
            ExchangeOption exchangeoption = db.ExchangeOptions.Find(id);
            if (exchangeoption == null)
            {
                return HttpNotFound();
            }
            return View(exchangeoption);
        }

        //
        // POST: /ExchangeOption/Delete/5

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ExchangeOption exchangeoption = db.ExchangeOptions.Find(id);
            db.ExchangeOptions.Remove(exchangeoption);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete exchange destination." + e.Message;
                return View(exchangeoption);
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}