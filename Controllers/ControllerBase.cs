using SchoolOfScience.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;

namespace SchoolOfScience.Controllers
{
    public class ControllerBase : Controller
    {
        private SchoolOfScienceEntities db = new SchoolOfScienceEntities();

        public bool IsEligible(Program program, StudentProfile student)
        {
            if (student != null && program != null)
            {
                return ((true)
                    && ((String.IsNullOrEmpty(program.eligible_academic_career)) || (program.eligible_academic_career.Split(',').Contains(student.academic_career)))
                    && ((String.IsNullOrEmpty(program.eligible_academic_organization)) || (program.eligible_academic_organization.Split(',').Contains(student.academic_organization)))
                    && ((String.IsNullOrEmpty(program.eligible_academic_plan)) || (program.eligible_academic_plan.Split(',').Contains(student.StudentAcademicPlan.major_plan1)
                        || (!String.IsNullOrEmpty(student.StudentAcademicPlan.major_plan2) && program.eligible_academic_plan.Split(',').Contains(student.StudentAcademicPlan.major_plan2))
                        || (!String.IsNullOrEmpty(student.StudentAcademicPlan.major_plan3) && program.eligible_academic_plan.Split(',').Contains(student.StudentAcademicPlan.major_plan3))
                        || (!String.IsNullOrEmpty(student.StudentAcademicPlan.major_plan4) && program.eligible_academic_plan.Split(',').Contains(student.StudentAcademicPlan.major_plan4))
                        ))
                    && ((String.IsNullOrEmpty(program.eligible_program_status)) || (program.eligible_program_status.Split(',').Contains(student.program_status)))
                    && ((String.IsNullOrEmpty(program.eligible_academic_level)) || (program.eligible_academic_level.Split(',').Contains(student.academic_level))));
            }
            else
            {
                return false;
            }
        }

        public Notification CreateNotification(String type, Application application)
        {
            NotificationType notificationtype = db.NotificationTypes.Where(t => t.name == type).SingleOrDefault();
            NotificationStatus notificationstatus = db.NotificationStatus.Where(s => s.name == "Pending").SingleOrDefault();
            if (notificationtype.NotificationTemplate != null)
            {
                string body = "";
                string subject = "";
                body = notificationtype.NotificationTemplate.body;
                body = body.Replace("[student id]", application.student_id);
                body = body.Replace("[student name]", application.StudentProfile.name);
                body = body.Replace("[application id]", application.id.ToString());
                body = body.Replace("[program id]", application.program_id.ToString());
                body = body.Replace("[program name]", application.Program.name);
                body = body.Replace("[submit date]", String.Format("{0:yyyy-MM-dd HH:mm:ss}", application.submitted));

                subject = notificationtype.NotificationTemplate.subject;
                subject = subject.Replace("[student id]", application.student_id);
                subject = subject.Replace("[student name]", application.StudentProfile.name);
                subject = subject.Replace("[application id]", application.id.ToString());
                subject = subject.Replace("[program id]", application.program_id.ToString());
                subject = subject.Replace("[program name]", application.Program.name);
                subject = subject.Replace("[submit date]", String.Format("{0:yyyy-MM-dd HH:mm:ss}", application.submitted));

                Notification notification = new Notification
                {
                    send_time = DateTime.Now,
                    sender = notificationtype.NotificationTemplate.sender,
                    subject = subject,
                    body = body,
                    status_id = notificationstatus.id,
                    type_id = notificationtype.id,
                    template_id = notificationtype.NotificationTemplate.id,
                    application_id = application.id,
                    created = DateTime.Now,
                    created_by = WebSecurity.CurrentUserName,
                    modified = DateTime.Now,
                    modified_by = WebSecurity.CurrentUserName
                };
                //db.Notifications.Add(notification);
                //db.SaveChanges();

                notification.NotificationRecipients.Add(new NotificationRecipient
                {
                    email = application.StudentProfile.email,
                    recipient_type = "to",
                    student_id = application.student_id
                });

                if (!String.IsNullOrEmpty(notificationtype.NotificationTemplate.cc))
                {
                    List<NotificationRecipient> ccList = new List<NotificationRecipient>();
                    notificationtype.NotificationTemplate.cc.Split(',').ToList().ForEach(s => ccList.Add(new NotificationRecipient { email = s.Trim(), recipient_type = "cc" }));
                    notification.NotificationRecipients = notification.NotificationRecipients.Concat(ccList).ToList();
                }

                if (!String.IsNullOrEmpty(notificationtype.NotificationTemplate.bcc))
                {
                    List<NotificationRecipient> bccList = new List<NotificationRecipient>();
                    notificationtype.NotificationTemplate.bcc.Split(',').ToList().ForEach(s => bccList.Add(new NotificationRecipient { email = s.Trim(), recipient_type = "bcc" }));
                    notification.NotificationRecipients = notification.NotificationRecipients.Concat(bccList).ToList();
                }

                db.Notifications.Add(notification);
                try
                {
                    db.SaveChanges();
                    return notification;
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] += "<br/><br/>Failed to create notification record. <br/><br/>" + e.Message;
                }
            }
            return null;
        }

        public void SendNotification(Notification notification)
        {
            if (notification != null)
            {
                try
                {
                    MailMessage mail = new MailMessage();
                    notification.NotificationRecipients.Where(r => r.recipient_type == "to").ToList().ForEach(r => mail.To.Add(r.email));
                    notification.NotificationRecipients.Where(r => r.recipient_type == "cc").ToList().ForEach(r => mail.CC.Add(r.email));
                    notification.NotificationRecipients.Where(r => r.recipient_type == "bcc").ToList().ForEach(r => mail.Bcc.Add(r.email));
                    //mail.From = new MailAddress(notification.sender);
                    mail.From = new MailAddress("advise@ust.hk");
                    mail.Subject = notification.subject;
                    string Body = notification.body;
                    mail.Body = Body;
                    mail.IsBodyHtml = true;
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.ust.hk";
                    smtp.Port = 25;
                    smtp.UseDefaultCredentials = false;
                    //smtp.Credentials = new System.Net.NetworkCredential("sswonghs", "1711jasj");// Enter senders User name and password
                    smtp.EnableSsl = true;
                    smtp.Send(mail);

                    NotificationStatus status = db.NotificationStatus.Where(s => s.name == "Sent").SingleOrDefault();
                    notification.status_id = status.id;
                    notification.send_time = DateTime.Now;
                    notification.modified = DateTime.Now;
                    notification.modified_by = WebSecurity.CurrentUserName;
                    db.Entry(notification).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Session["FlashMessage"] += "<br/><br/>Failed to send notifications. <br/><br/>" + e.Message;
                }
            }
        }

        //convert a razor view to string, for excel export
        public string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}
