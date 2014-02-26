using SchoolOfScience.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SchoolOfScience.Controllers
{
    public class FileHandlerController : Controller
    {
        [Authorize]
        public ActionResult Upload(string elementId = null, string folder = null)
        {
            ViewBag.elementId = elementId;
            ViewBag.Success = false;
            return PartialView();
        }

        [HttpPost]
        [Authorize]
        [ValidateInput(false)]
        public ActionResult Upload(HttpPostedFileBase file, string elementId = null, string folder = null, int programid = 0, int applicationid = 0, int advisingremarkid = 0)
        {
            try
            {
                ViewBag.Success = false;
                if (file == null)
                {
                    Session["FlashMessage"] = ("No file seleced.");
                    return View();
                }
                if (file.ContentLength > 0 && !String.IsNullOrEmpty(folder))
                {
                    var path = Server.MapPath("~/App_Data/" + folder);
                    var filename = HttpUtility.UrlEncode(Path.GetFileName(file.FileName), System.Text.Encoding.UTF8);
                    var filepath = Path.Combine(path, filename);

                    if (file.ContentLength > 20480000)
                    {
                        Session["FlashMessage"] = ("File size exceeded system's limit.");
                        return View();
                    }

                    if (System.IO.File.Exists(filepath)
                        || System.IO.File.Exists(Path.Combine(Server.MapPath("~/App_Data/"), "Attachments/Program/", programid.ToString(), filename))
                        || System.IO.File.Exists(Path.Combine(Server.MapPath("~/App_Data/"), "Attachments/Application/", applicationid.ToString(), filename))
                        || System.IO.File.Exists(Path.Combine(Server.MapPath("~/App_Data/"), "Attachments/AdvisingRemark/", advisingremarkid.ToString(), filename))
                        )
                    {
                        Session["FlashMessage"] = ("You have uploaded a file with the same name. Please check your file.<br/>If the error continues, please save your application and upload the file again.");
                        return View();
                    }

                    Directory.CreateDirectory(path);
                    file.SaveAs(filepath);
                    ViewBag.filename = filename;
                    ViewBag.folder = folder;
                    ViewBag.elementId = elementId;
                    ViewBag.Success = true;
                }
                return PartialView();
            }
            catch (Exception e)
            {
                Session["FlashMessage"] = ("File upload failed. " + e.Message );
                return View();
            }
        }

        [Authorize]
        public ActionResult Error()
        {
            return Content("<script>alert('File size exceeded server's limit.'); window.close();</script>");
        }

        [Authorize]
        [ValidateInput(false)]
        public ActionResult Download(string filename, string folder)
        {
            var path = Server.MapPath("~/App_Data/" + folder);
            var filepath = Path.Combine(path, filename);
            
            if (!System.IO.File.Exists(filepath))
            {
                return Content("<script>alert('File not exist.'); window.close();</script>");
            }
            
            return File(filepath, "application/octet-stream", Path.GetFileName(filepath));
            
        }

        //[Ajax(true)]
        //[Authorize]
        //public void Delete(string filename, string folder)
        //{
        //    var path = Server.MapPath("~/App_Data/" + folder);
        //    var filepath = Path.Combine(path, filename);
        //    if (System.IO.File.Exists(filepath))
        //    {
        //        System.IO.File.Delete(filepath);
        //    }
        //}
    }
}
