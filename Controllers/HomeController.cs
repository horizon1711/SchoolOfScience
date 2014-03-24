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
            if (Roles.IsUserInRole("StudentUGRD"))
            {
                return View("HomeStudent");
            }
            if (Roles.IsUserInRole("StudentRPGTPG"))
            {
                return View("HomeStudent");
            }
            if (Roles.IsUserInRole("StudentNUGD"))
            {
                return View("HomeStudent");
            }
            if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("Advising") || Roles.IsUserInRole("StudentDevelopment"))
            {
                return View("HomeAdmin");
            }
            if (Roles.IsUserInRole("FacultyAdvisor"))
            {
                return View("HomeFaculty");
            }
            if (Roles.IsUserInRole("EDP"))
            {
                return View("HomeEDP");
            }
            if (Roles.IsUserInRole("CommTutor"))
            {
                return View("HomeCommTutor");
            }
            if (Roles.IsUserInRole("Nominator"))
            {
                return View("HomeNominator");
            }
            if (Roles.IsUserInRole("ProgramAdmin"))
            {
                return View("HomeProgramAdmin");
            }
            return View();
        }

        [Authorize]
        public ActionResult ComingSoon()
        {
            return View();
        }



        //[Authorize]
        public ActionResult Menu()
        {
            SchoolOfScienceEntities db = new SchoolOfScienceEntities();
            ViewBag.programTypes = db.ProgramTypes.Where(t => t.display_on_menu);
            Roles.GetRolesForUser(User.Identity.Name);
            if (Roles.IsUserInRole("Admin") || Roles.IsUserInRole("Advising") || Roles.IsUserInRole("StudentDevelopment"))
            {
                return View("MenuAdmin");
            }
            if (Roles.IsUserInRole("EDP"))
            {
                return View("MenuEDP");
            }
            if (Roles.IsUserInRole("CommTutor"))
            {
                return View("MenuCommTutor");
            }
            if (Roles.IsUserInRole("FacultyAdvisor"))
            {
                return View("MenuFacultyAdvisor");
            }
            if (Roles.IsUserInRole("ProgramAdmin"))
            {
                return View("MenuProgramAdmin");
            }
            if (Roles.IsUserInRole("Nominator"))
            {
                return View("MenuNominator");
            }
            if (Roles.IsUserInRole("StudentUGRD"))
            {
                return View("MenuStudent");
            }
            if (Roles.IsUserInRole("StudentRPGTPG"))
            {
                return View("MenuStudent");
            }
            if (Roles.IsUserInRole("StudentNUGD"))
            {
                return View("MenuStudent");
            }
            return View();
        }

        [Authorize]
        public ActionResult EmailTest()
        {
            EmailViewModel ViewModel = new EmailViewModel { 
                host = "smtp.gmail.com", 
                port = 587, 
                subject = "Testing Message from School Of Science Application",
                body = "This is a <i>testing messsage</i> from ASP.NET MVC 4" 
            };
            return View(ViewModel);
        }

        [HttpPost, ValidateInput(false)]
        [Authorize]
        public ActionResult EmailTest(EmailViewModel ViewModel)
        {
            if (ModelState.IsValid)
            {
                MailMessage mail = new MailMessage();
                mail.To.Add(ViewModel.to);
                mail.From = new MailAddress(ViewModel.from);
                mail.Subject = ViewModel.subject;
                string Body = ViewModel.body;
                mail.Body = Body;
                mail.IsBodyHtml = true;
                SmtpClient smtp = new SmtpClient();
                smtp.Host = ViewModel.host;
                smtp.Port = ViewModel.port;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(ViewModel.username, ViewModel.password);// Enter seders User name and password
                smtp.EnableSsl = true;
                smtp.Send(mail);
            }

            return View(ViewModel);
        }
    }
}
