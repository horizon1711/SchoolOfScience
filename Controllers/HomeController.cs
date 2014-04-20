using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using SchoolOfScience.Models;
using SchoolOfScience.Models.ViewModels;
using WebMatrix.WebData;
using System.Web.Security;
using SchoolOfScience.Filters;

namespace SchoolOfScience.Controllers
{
    [InitializeSimpleMembership]
    public class HomeController : Controller
    {
        private UsersContext userdb = new UsersContext();

        [Authorize]
        public ActionResult Index()
        {
            if (Roles.IsUserInRole("Admin")
                || Roles.IsUserInRole("Advising")
                || Roles.IsUserInRole("StudentDevelopment")
                || Roles.IsUserInRole("FacultyAdvisor")
                || Roles.IsUserInRole("EDP")
                || Roles.IsUserInRole("Nominator")
                || Roles.IsUserInRole("ProgramAdmin")
                || Roles.IsUserInRole("ProgramViewer")
                || Roles.IsUserInRole("CommTutor")
                || Roles.IsUserInRole("UGCoordinator")
                || Roles.IsUserInRole("StudentUGRD")
                || Roles.IsUserInRole("StudentRPGTPG")
                || Roles.IsUserInRole("StudentNUGD"))
            {
                return View("Home");
            }
            Session["FlashMessage"] = "User not in any role of the system. Please login using another credential.";
            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        public ActionResult ComingSoon()
        {
            return View();
        }



        //[Authorize]
        public ActionResult Menu()
        {
            if (Roles.IsUserInRole("Admin")
                || Roles.IsUserInRole("Advising")
                || Roles.IsUserInRole("StudentDevelopment")
                || Roles.IsUserInRole("FacultyAdvisor")
                || Roles.IsUserInRole("EDP")
                || Roles.IsUserInRole("Nominator")
                || Roles.IsUserInRole("ProgramAdmin")
                || Roles.IsUserInRole("ProgramViewer")
                || Roles.IsUserInRole("Nominator")
                || Roles.IsUserInRole("CommTutor")
                || Roles.IsUserInRole("UGCoordinator")
                || Roles.IsUserInRole("StudentUGRD")
                || Roles.IsUserInRole("StudentRPGTPG")
                || Roles.IsUserInRole("StudentNUGD"))
            {
                SchoolOfScienceEntities db = new SchoolOfScienceEntities();
                ViewBag.programTypes = db.ProgramTypes.Where(t => t.display_on_menu);
                return View("Menu");
            }
            return View("MenuEmpty");
        }

        //[Authorize]
        //public ActionResult EmailTest()
        //{
        //    EmailViewModel ViewModel = new EmailViewModel { 
        //        host = "smtp.gmail.com", 
        //        port = 587, 
        //        subject = "Testing Message from School Of Science Application",
        //        body = "This is a <i>testing messsage</i> from ASP.NET MVC 4" 
        //    };
        //    return View(ViewModel);
        //}

        //[HttpPost, ValidateInput(false)]
        //[Authorize]
        //public ActionResult EmailTest(EmailViewModel ViewModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        MailMessage mail = new MailMessage();
        //        mail.To.Add(ViewModel.to);
        //        mail.From = new MailAddress(ViewModel.from);
        //        mail.Subject = ViewModel.subject;
        //        string Body = ViewModel.body;
        //        mail.Body = Body;
        //        mail.IsBodyHtml = true;
        //        SmtpClient smtp = new SmtpClient();
        //        smtp.Host = ViewModel.host;
        //        smtp.Port = ViewModel.port;
        //        smtp.UseDefaultCredentials = false;
        //        smtp.Credentials = new System.Net.NetworkCredential(ViewModel.username, ViewModel.password);// Enter seders User name and password
        //        smtp.EnableSsl = true;
        //        smtp.Send(mail);
        //    }

        //    return View(ViewModel);
        //}
    }
}
