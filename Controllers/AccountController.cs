using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using SchoolOfScience.Filters;
using SchoolOfScience.Models;
using SchoolOfScience.Attributes;
using System.IO;
using System.Net;
using System.Xml;

namespace SchoolOfScience.Controllers
{
    [Authorize]
    [InitializeSimpleMembership]
    public class AccountController : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();
        //
        // GET: /Account/Login


        private const string CASHOST = "https://cas.ust.hk/cas/";

        // Or you could use the appsettings
        // private const string CASHOST = ConfigurationManager.AppSettings["CASHOST"];

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.Url.Host == "sdb.science.ust.hk" || Request.Url.Host == "sfs-dev1.ust.hk")
            {

                // Look for the "ticket=" after the "?" in the URL
                string AuthorisationTicket = Request.QueryString["ticket"];

                // This page is the CAS service=, but discard any query string residue
                //        string UrlToAuthenticate = Request.Url.GetLeftPart(UriPartial.Path);
                //string UrlToAuthenticate = "https://sdb.science.ust.hk" + Request.RawUrl;
                //string UrlToAuthenticate = Request.Url.Scheme + "://" + Request.Url.Host + Request.RawUrl;
                string UrlToAuthenticate = Request.Url.AbsoluteUri;

                // Look for a Session object called CASId
                if (Session["CASId"] != null)
                {
                    // If the user gets here then either their session has timed out,
                    // they have possibly logged out or the role provider has denied
                    // them access to the page they have requested

                    // Users will always return here if they do not have role
                    // permission to view a page. The downside is, it will sign
                    // them out of pages they DO have access to and they'll have to
                    // start again.

                    // Double check they are signed out
                    string SignoutUrl = CASHOST + "logout";
                    StreamReader SignoutHttpReader = new StreamReader(new WebClient().OpenRead(SignoutUrl));
                    string SignOutResponse = SignoutHttpReader.ReadToEnd();

                    // Sign the user out of the .NET site and Give them a message,
                    // they'll never see it though...
                    Session.Abandon();
                    Session.Clear();
                    FormsAuthentication.SignOut();

                    // ...because they get redirected and forced to renew their
                    // CAS authentication ticket.
                    string RenewSignonUrl = CASHOST + "login?" + "service=" + UrlToAuthenticate + "&renew=true";
                    //Response.Redirect(RenewSignonUrl);
                    return Redirect(RenewSignonUrl);
                }
                else
                {

                    // First time through there is no ticket=, so redirect to CAS login
                    if (AuthorisationTicket == null || AuthorisationTicket.Length == 0)
                    {
                        string SignonUrl = CASHOST + "login?" + "service=" + UrlToAuthenticate;
                        //Response.Redirect(SignonUrl);
                        //return;
                        return Redirect(SignonUrl);
                    }

                    // Second time (back from CAS) there is a ticket= to validate
                    string ValidateSignonUrl = CASHOST + "serviceValidate?" + "ticket=" + AuthorisationTicket + "&" + "service=" + UrlToAuthenticate;
                    StreamReader ValidateSignonHttpReader = new StreamReader(new WebClient().OpenRead(ValidateSignonUrl));

                    // I like to have the text in memory for debugging rather than parsing the stream
                    string ValidateSignonResponse = ValidateSignonHttpReader.ReadToEnd();

                    // Some boilerplate to set up the parse.
                    NameTable XmlNT = new NameTable();
                    XmlNamespaceManager XmlNSManager = new XmlNamespaceManager(XmlNT);
                    XmlParserContext XmlParser = new XmlParserContext(null, XmlNSManager, null, XmlSpace.None);
                    XmlTextReader XmlResponseReader = new XmlTextReader(ValidateSignonResponse, XmlNodeType.Element, XmlParser);

                    string SSOUsername = null;

                    // A very dumb use of XML. Just scan for the "user". If it isn't there, its an error.
                    while (XmlResponseReader.Read())
                    {
                        if (XmlResponseReader.IsStartElement())
                        {
                            string XmlTag = XmlResponseReader.LocalName;
                            if (XmlTag == "user")
                            {
                                SSOUsername = XmlResponseReader.ReadString();
                            }
                        }
                    }
                    // if you want to parse the proxy chain, just add the logic above
                    XmlResponseReader.Close();

                    // If there was a problem, leave the message on the screen. Otherwise, return to original page.
                    if (SSOUsername == null)
                    {
                        Session["FlashMessage"] = "Login failed. </br><br/>For science staff and students, please email us at <a href='mailto:advise@ust.hk'>advise@ust.hk</a> to resolve this issue.";
                        return View("CASLoginFail");
                    }
                    else
                    {
                        // Create a session for recording the fact that we are logged on
                        // and for allowing us to use .NET role management.

                        // Was going to store AuthorisationTicket here but not sure if that
                        // is good practice or not.
                        Session["CASId"] = SSOUsername;

                        // set SSOUsername in ASP.NET blocks
                        //FormsAuthentication.RedirectFromLoginPage(SSOUsername, false);

                        //MVC Application login
                        if (WebSecurity.Login(SSOUsername, "100100", false))
                        {
                            if (!String.IsNullOrEmpty(returnUrl))
                            {
                                return RedirectToLocal(returnUrl);
                            }
                            else
                            {
                                return RedirectToAction("Index", "Home");
                            }
                        }
                        else
                        {
                            var student = db.StudentProfiles.SingleOrDefault(s => s.email.Substring(0, s.email.IndexOf("@")) == SSOUsername);

                            if (student != null)
                            {
                                //MVC Application login
                                if (WebSecurity.Login(student.id, "100100", false))
                                {
                                    Session["StudentName"] = student.name;
                                    if (!String.IsNullOrEmpty(returnUrl))
                                    {
                                        return RedirectToLocal(returnUrl);
                                    }
                                    else
                                    {
                                        return RedirectToAction("Index", "Home");
                                    }
                                }
                                else
                                {
                                    WebSecurity.CreateUserAndAccount(student.id, "100100");

                                    if (student.academic_career == "UGRD")
                                    {
                                        // Add Role to User
                                        Roles.AddUserToRole(student.id, "StudentUGRD");
                                    }
                                    if (student.academic_career == "RPG")
                                    {
                                        // Add Role to User
                                        Roles.AddUserToRole(student.id, "StudentRPGTPG");
                                    }
                                    if (student.academic_career == "TPG")
                                    {
                                        // Add Role to User
                                        Roles.AddUserToRole(student.id, "StudentRPGTPG");
                                    }
                                    if (student.academic_career == "NUGD")
                                    {
                                        // Add Role to User
                                        Roles.AddUserToRole(student.id, "StudentNUGD");
                                    }

                                    if (WebSecurity.Login(student.id, "100100"))
                                    {
                                        Session["StudentName"] = student.name;
                                        if (!String.IsNullOrEmpty(returnUrl))
                                        {
                                            return RedirectToLocal(returnUrl);
                                        }
                                        else
                                        {
                                            return RedirectToAction("Index", "Home");
                                        }
                                    }
                                }
                            }
                            Session.Remove("StudentName");
                            Session["FlashMessage"] = "Login failed. </br><br/>For science staff and students, please email us at <a href='mailto:advise@ust.hk'>advise@ust.hk</a> to resolve this issue.";
                            return View("CASLoginFail");
                        }
                    }
                    // Note: if you use the asp:LoginStatus control, remember the LogoutPageUrl
                    // should be set to the CAS logout page and you should set the LogoutAction to Redirect
                }
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }
        }
        //
        // GET: /Account/Login

        [Ajax(true)]
        public ActionResult Login()
        {
            return HttpNotFound("Session timeout / Not Authorized. <br/>Please <a href='javascript:void(0)' onclick='window.location.reload()'>refresh</a> to login.");
        }

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl)
        {
            model.Password = "100100";
            model.RememberMe = true;
            if (WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {
                //string roles = String.Join(",", Roles.GetRolesForUser(model.UserName));
                //FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                //  1, 
                //  model.UserName,  //user id
                //  DateTime.Now,
                //  DateTime.Now.AddMinutes(600),  // expiry
                //  false,  //do not remember
                //  roles,
                //  "/");
                //HttpCookie cookie = new HttpCookie(FormsAuthentication.FormsCookieName,
                //                                   FormsAuthentication.Encrypt(authTicket));
                //cookie.Expires = DateTime.Now.AddMinutes(600);
                //Response.Cookies.Add(cookie);
                StudentProfile student = db.StudentProfiles.Find(model.UserName);
                if (student != null)
                {
                    Session["StudentName"] = student.name;
                }
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // Attempt to register the user
                try
                {
                    StudentProfile student = db.StudentProfiles.Find(model.UserName);
                    if (student != null)
                    {
                        WebSecurity.CreateUserAndAccount(model.UserName, model.Password);

                        if (student.academic_career == "UGRD")
                        {
                            // Add Role to User
                            Roles.AddUserToRole(model.UserName, "StudentUGRD");
                        }
                        if (student.academic_career == "RPG")
                        {
                            // Add Role to User
                            Roles.AddUserToRole(model.UserName, "StudentRPGTPG");
                        }
                        if (student.academic_career == "TPG")
                        {
                            // Add Role to User
                            Roles.AddUserToRole(model.UserName, "StudentRPGTPG");
                        }
                        if (student.academic_career == "NUGD")
                        {
                            // Add Role to User
                            Roles.AddUserToRole(model.UserName, "StudentNUGD");
                        }

                        WebSecurity.Login(model.UserName, model.Password);

                        Session["StudentName"] = student.name;
                        return RedirectToAction("Index", "Home");
                    }
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }


            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            Session.Remove("StudentName");
            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff(int id = 0)
        {
            return RedirectToAction("Index", "Home");
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            //Session.Abandon();
            //Session.Clear();
            WebSecurity.Logout();

            return RedirectToAction("Login", new { returnUrl = Url.Action("Index", "Home") });
        }


        // Create Role List for Registration
        private SelectList getRoleSelectList()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            items.Add(new SelectListItem { Text = "Administrator", Value = "Admin" });
            items.Add(new SelectListItem { Text = "Advising Team", Value = "Advising" });
            items.Add(new SelectListItem { Text = "Student Development Team", Value = "StudentDevelopment" });
            items.Add(new SelectListItem { Text = "Faculty Advisors (Pre-major Faculty Advisor/Faculty Mentor)", Value = "FacultyAdvisor" });
            items.Add(new SelectListItem { Text = "EDP Team", Value = "EDP" });
            items.Add(new SelectListItem { Text = "Comm Tutor", Value = "CommTutor" });
            items.Add(new SelectListItem { Text = "Program Admin", Value = "ProgramAdmin" });
            items.Add(new SelectListItem { Text = "Nominator", Value = "Nominator" });
            items.Add(new SelectListItem { Text = "Students (UGRD)", Value = "StudentUGRD" });
            items.Add(new SelectListItem { Text = "Students (RPG,TPG)", Value = "StudentRPGTPG" });
            items.Add(new SelectListItem { Text = "Students (NUGD)", Value = "StudentNUGD" });

            return new SelectList(items,"Value","Text");
        }

        //
        // GET: /Account/Register

        [Authorize(Roles="Admin,Advising,StudentDevelopment")]
        public ActionResult Register()
        {
            ViewBag.Role = getRoleSelectList();

            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            model.Password = "100100";
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password);

                    if (String.IsNullOrEmpty(model.Role))
                    {
                        var roles = Roles.GetRolesForUser(model.UserName);
                        foreach (var role in roles)
                        {
                            Roles.RemoveUserFromRole(model.UserName, role);
                        }
                    }
                    else
                    {
                        // Add Role to User
                        if (!Roles.RoleExists(model.Role))
                            Roles.CreateRole(model.Role);
                        Roles.AddUserToRole(model.UserName, model.Role);
                    }

                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }

            ViewBag.Role = getRoleSelectList();

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [Authorize(Roles = "Admin,Advising,StudentDevelopment")]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
