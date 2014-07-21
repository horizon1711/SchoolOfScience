using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using System.IO;

namespace SchoolOfScience.Controllers
{
    [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
    public class ProgramTypeController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        //
        // GET: /ProgramType/

        public ActionResult Index()
        {
            return View(db.ProgramTypes.ToList());
        }

        //
        // GET: /ProgramType/Details/5

        public ActionResult Details(int id = 0)
        {
            ProgramType programtype = db.ProgramTypes.Find(id);
            if (programtype == null)
            {
                Session["FlashMessage"] = "Program Type not found.";
                return RedirectToAction("Index");
            }
            return View(programtype);
        }

        //
        // GET: /ProgramType/Create

        public ActionResult Create()
        {
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "Application End Time", Value = "deadline", Selected = true });
            items.Add(new SelectListItem { Text = "Program Date", Value = "program" });

            ViewBag.display_date = items;
            return View();
        }

        //
        // POST: /ProgramType/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProgramType programtype)
        {
            if (ModelState.IsValid)
            {
                db.ProgramTypes.Add(programtype);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to create type." + e.Message;
                    List<SelectListItem> items = new List<SelectListItem>();
                    items.Add(new SelectListItem { Text = "Application End Time", Value = "deadline", Selected = true });
                    items.Add(new SelectListItem { Text = "Program Date", Value = "program" });

                    ViewBag.display_date = items;
                    return View(programtype);
                }
                return RedirectToAction("Index");
            }

            return View(programtype);
        }

        //
        // GET: /ProgramType/Edit/5

        public ActionResult Edit(int id = 0)
        {
            ProgramType programtype = db.ProgramTypes.Find(id);
            if (programtype == null)
            {
                Session["FlashMessage"] = "Program Type not found.";
                return RedirectToAction("Index");
            }
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "Application End Time", Value = "deadline" });
            items.Add(new SelectListItem { Text = "Program Date", Value = "program" });

            ViewBag.dateTypeList = items;
            return View(programtype);
        }

        //
        // POST: /ProgramType/Edit/5

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProgramType programtype)
        {
            if (ModelState.IsValid)
            {
                if (!String.IsNullOrEmpty(programtype.image_filename))
                {
                    var sourcePath = Server.MapPath("~/App_Data/" + programtype.image_filepath);
                    var sourceFilepath = Path.Combine(sourcePath, programtype.image_filename);
                    var destPath = Server.MapPath("~/Images/ProgramType/" + programtype.id.ToString());
                    var destFilepath = Path.Combine(destPath, programtype.image_filename);
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception e)
                    {
                        Session["FlashMessage"] = "Failed to create directory. Please check write permission of ~/Images/ProgramType. <br/><br/>" + e.Message;
                    }
                    try
                    {
                        if (!System.IO.File.Exists(destFilepath))
                        {
                            System.IO.File.Move(sourceFilepath, destFilepath);
                            programtype.image_filepath = "~/Images/ProgramType/" + programtype.id.ToString();
                        }
                    }
                    catch (Exception e)
                    {
                        Session["FlashMessage"] = "Failed to move file. Please check write permission of ~/Images/ProgramType. <br/><br/>" + e.Message;
                    }
                }
                //clear temp files uploaded but not used
                if (Directory.Exists(Server.MapPath("~/App_Data/Temp/ProgramType/" + programtype.id.ToString())))
                {
                    var files = Directory.GetFiles(Server.MapPath("~/App_Data/Temp/ProgramType/" + programtype.id.ToString()));
                    foreach (var file in files)
                    {
                        System.IO.File.Delete(file);
                    }
                }

                db.Entry(programtype).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] = "Failed to update type." + e.Message;
                    List<SelectListItem> items = new List<SelectListItem>();
                    items.Add(new SelectListItem { Text = "Application End Time", Value = "deadline", Selected = true });
                    items.Add(new SelectListItem { Text = "Program Date", Value = "program" });

                    ViewBag.display_date = items;
                    return View(programtype);
                }
                return RedirectToAction("Index");
            }
            return View(programtype);
        }

        //
        // GET: /ProgramType/Delete/5

        public ActionResult Delete(int id = 0)
        {
            ProgramType programtype = db.ProgramTypes.Find(id);
            if (programtype == null)
            {
                Session["FlashMessage"] = "Program Type not found.";
                return RedirectToAction("Index");
            }
            if (programtype.Programs.Count() > 0)
            {
                Session["FlashMessage"] = "<b>Program Type is attached to existing Program(s).</b> <br/>";
                programtype.Programs.ToList().ForEach(p => Session["FlashMessage"] += "<i>" + p.name + "</i><br/>");
                return RedirectToAction("Index");
            }
            return View(programtype);
        }

        //
        // POST: /ProgramType/Delete/5

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProgramType programtype = db.ProgramTypes.Find(id);
            db.ProgramTypes.Remove(programtype);
            try
            {
                db.SaveChanges();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = "Failed to delete type." + e.Message;
                return View("Delete", programtype);
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