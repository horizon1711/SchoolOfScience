﻿using SchoolOfScience.Models;
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

            if (type == "ApplicationSubmitted")
            {
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
                else
                {
                    Session["FlashMessage"] += "<br/><br/>Notification Template is not correctly configured";
                }
            }

            if (type == "ApplicationSavedReminder")
            {
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
                    body = body.Replace("[program deadline]", String.Format("{0:yyyy-MM-dd HH:mm}", application.Program.application_end_time));
                    body = body.Replace("[submit date]", String.Format("{0:yyyy-MM-dd HH:mm:ss}", application.submitted));

                    subject = notificationtype.NotificationTemplate.subject;
                    subject = subject.Replace("[student id]", application.student_id);
                    subject = subject.Replace("[student name]", application.StudentProfile.name);
                    subject = subject.Replace("[application id]", application.id.ToString());
                    subject = subject.Replace("[program id]", application.program_id.ToString());
                    subject = subject.Replace("[program name]", application.Program.name);
                    subject = subject.Replace("[program deadline]", String.Format("{0:yyyy-MM-dd HH:mm}", application.Program.application_end_time));
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
                else
                {
                    Session["FlashMessage"] += "<br/><br/>Notification Template is not correctly configured";
                }
            }
            return null;
        }
        
        public Notification CreateNotification(String type, Program program)
        {
            NotificationType notificationtype = db.NotificationTypes.Where(t => t.name == type).SingleOrDefault();
            NotificationStatus notificationstatus = db.NotificationStatus.Where(s => s.name == "Pending").SingleOrDefault();
            if (type == "ProgramPublished")
            {
                if (notificationtype.NotificationTemplate != null)
                {
                    string body = "";
                    string subject = "";
                    string directlink = "https://sdb.science.ust.hk/mySCI/Program/Showcase/" + program.id.ToString();
                    body = notificationtype.NotificationTemplate.body;
                    body = body.Replace("[program id]", program.id.ToString());
                    body = body.Replace("[program name]", program.name);
                    body = body.Replace("[program period]", program.start_time);
                    body = body.Replace("[program deadline]", String.Format("{0:yyyy-MM-dd HH:mm}", program.application_end_time));
                    body = body.Replace("[program directlink]", "<a href='" + directlink + "' target='_blank'>" + directlink + "</a>");

                    subject = notificationtype.NotificationTemplate.subject;
                    subject = subject.Replace("[program id]", program.id.ToString());
                    subject = subject.Replace("[program name]", program.name);
                    subject = subject.Replace("[program period]", program.start_time);
                    subject = subject.Replace("[program deadline]", String.Format("{0:yyyy-MM-dd HH:mm}", program.application_end_time));
                    subject = subject.Replace("[program directlink]", "<a href='" + directlink + "' target='_blank'>" + directlink + "</a>");

                    Notification notification = new Notification
                    {
                        send_time = DateTime.Now,
                        sender = notificationtype.NotificationTemplate.sender,
                        subject = subject,
                        body = body,
                        status_id = notificationstatus.id,
                        type_id = notificationtype.id,
                        template_id = notificationtype.NotificationTemplate.id,
                        program_id = program.id,
                        created = DateTime.Now,
                        created_by = WebSecurity.CurrentUserName,
                        modified = DateTime.Now,
                        modified_by = WebSecurity.CurrentUserName
                    };
                    //db.Notifications.Add(notification);
                    //db.SaveChanges();

                    foreach (var student in db.StudentProfiles)
                    {
                        if (IsEligible(program, student))
                        {
                            notification.NotificationRecipients.Add(new NotificationRecipient
                            {
                                email = student.email,
                                recipient_type = "bcc",
                                student_id = student.id
                            });
                        }
                    }

                    if (!String.IsNullOrEmpty(notificationtype.NotificationTemplate.cc))
                    {
                        List<NotificationRecipient> ccList = new List<NotificationRecipient>();
                        notificationtype.NotificationTemplate.cc.Split(',').ToList().ForEach(s => ccList.Add(new NotificationRecipient { email = s.Trim(), recipient_type = "cc" }));
                        notification.NotificationRecipients = notification.NotificationRecipients.Concat(ccList).ToList();
                    }

                    if (!String.IsNullOrEmpty(notificationtype.NotificationTemplate.bcc))
                    {
                        List<NotificationRecipient> bccList = new List<NotificationRecipient>();
                        notificationtype.NotificationTemplate.bcc.Split(',').ToList().ForEach(s => bccList.Add(new NotificationRecipient { email = s.Trim(), recipient_type = "to" }));//change "bcc" to "to" for mass mail
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
                else
                {
                    Session["FlashMessage"] += "<br/><br/>Notification Template is not correctly configured";
                }
            }
            if (type == "ProgramDeadlindReminder")
            {
                if (notificationtype.NotificationTemplate != null)
                {
                    string body = "";
                    string subject = "";
                    string directlink = "https://sdb.science.ust.hk/mySCI/Program/Showcase/" + program.id.ToString();
                    body = notificationtype.NotificationTemplate.body;
                    body = body.Replace("[program id]", program.id.ToString());
                    body = body.Replace("[program name]", program.name);
                    body = body.Replace("[program period]", program.start_time);
                    body = body.Replace("[program deadline]", String.Format("{0:yyyy-MM-dd HH:mm}", program.application_end_time));
                    body = body.Replace("[program directlink]", "<a href='" + directlink + "' target='_blank'>" + directlink + "</a>");

                    subject = notificationtype.NotificationTemplate.subject;
                    subject = subject.Replace("[program id]", program.id.ToString());
                    subject = subject.Replace("[program name]", program.name);
                    subject = subject.Replace("[program period]", program.start_time);
                    subject = subject.Replace("[program deadline]", String.Format("{0:yyyy-MM-dd HH:mm}", program.application_end_time));
                    subject = subject.Replace("[program directlink]", "<a href='" + directlink + "' target='_blank'>" + directlink + "</a>");

                    Notification notification = new Notification
                    {
                        send_time = DateTime.Now,
                        sender = notificationtype.NotificationTemplate.sender,
                        subject = subject,
                        body = body,
                        status_id = notificationstatus.id,
                        type_id = notificationtype.id,
                        template_id = notificationtype.NotificationTemplate.id,
                        program_id = program.id,
                        created = DateTime.Now,
                        created_by = WebSecurity.CurrentUserName,
                        modified = DateTime.Now,
                        modified_by = WebSecurity.CurrentUserName
                    };
                    //db.Notifications.Add(notification);
                    //db.SaveChanges();
                    foreach (var student in db.StudentProfiles)
                    {
                        if (IsEligible(program, student) && !student.Applications.Any(a => a.program_id == program.id))
                        {
                            notification.NotificationRecipients.Add(new NotificationRecipient
                            {
                                email = student.email,
                                recipient_type = "bcc",
                                student_id = student.id
                            });
                        }
                    }

                    if (!String.IsNullOrEmpty(notificationtype.NotificationTemplate.cc))
                    {
                        List<NotificationRecipient> ccList = new List<NotificationRecipient>();
                        notificationtype.NotificationTemplate.cc.Split(',').ToList().ForEach(s => ccList.Add(new NotificationRecipient { email = s.Trim(), recipient_type = "cc" }));
                        notification.NotificationRecipients = notification.NotificationRecipients.Concat(ccList).ToList();
                    }

                    if (!String.IsNullOrEmpty(notificationtype.NotificationTemplate.bcc))
                    {
                        List<NotificationRecipient> bccList = new List<NotificationRecipient>();
                        notificationtype.NotificationTemplate.bcc.Split(',').ToList().ForEach(s => bccList.Add(new NotificationRecipient { email = s.Trim(), recipient_type = "to" }));//change "bcc" to "to" for mass mail
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
                else
                {
                    Session["FlashMessage"] += "<br/><br/>Notification Template is not correctly configured";
                }
            }
            return null;
        }

        public Notification CreateNotification(String type, Interview interview, Application application)
        {
            NotificationType notificationtype = db.NotificationTypes.Where(t => t.name == type).SingleOrDefault();
            NotificationStatus notificationstatus = db.NotificationStatus.Where(s => s.name == "Pending").SingleOrDefault();
            if (type == "InterviewAssigned")
            {
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
                    body = body.Replace("[interview date]", String.Format("{0:yyyy-MM-dd}", interview.start_time));
                    body = body.Replace("[interview time]", String.Format("{0:HH:mm}", interview.start_time) + " to " + String.Format("{0:HH:mm}", interview.end_time));
                    body = body.Replace("[interview venue]", interview.InterviewVenue.name);

                    subject = notificationtype.NotificationTemplate.subject;
                    subject = subject.Replace("[student id]", application.student_id);
                    subject = subject.Replace("[student name]", application.StudentProfile.name);
                    subject = subject.Replace("[application id]", application.id.ToString());
                    subject = subject.Replace("[program id]", application.program_id.ToString());
                    subject = subject.Replace("[program name]", application.Program.name);
                    subject = subject.Replace("[submit date]", String.Format("{0:yyyy-MM-dd HH:mm:ss}", application.submitted));
                    subject = subject.Replace("[interview date]", String.Format("{0:yyyy-MM-dd}", interview.start_time));
                    subject = subject.Replace("[interview time]", String.Format("{0:HH:mm}", interview.start_time) + " to " + String.Format("{0:HH:mm}", interview.end_time));
                    subject = subject.Replace("[interview venue]", interview.InterviewVenue.name);

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
                else
                {
                    Session["FlashMessage"] += "<br/><br/>Notification Template is not correctly configured";
                }
            }
            return null;
        }

        public Notification SendNotification(Notification notification)
        {
            bool emailsent = false;
            if (notification != null)
            {
                try
                {
                    MailMessage mail = new MailMessage();
                    //mail.From = new MailAddress(notification.sender);
                    mail.From = new MailAddress("advise@ust.hk");
                    mail.Subject = notification.subject;
                    string Body = notification.body;
                    mail.Body = Body;
                    mail.IsBodyHtml = true;

                    //testing server mail setup
                    SmtpClient smtp = new SmtpClient();
                    smtp.Host = "smtp.gmail.com";
                    smtp.Port = 587;
                    smtp.Credentials = new System.Net.NetworkCredential("treephoning@gmail.com", "16121612");// Enter senders User name and password
                    smtp.EnableSsl = true;

                    //production and development server mail setup
                    if (Request.Url.Host == "sdb.science.ust.hk" || Request.Url.Host == "sfs-dev1.ust.hk")
                    {
                        smtp.Host = "smtp.ust.hk";
                        smtp.Port = 25;
                        smtp.Credentials = null;
                        smtp.UseDefaultCredentials = false;
                    }

                    //add recipients to mail object
                    notification.NotificationRecipients.Where(r => r.recipient_type == "to").ToList().ForEach(r => mail.To.Add(r.email));
                    notification.NotificationRecipients.Where(r => r.recipient_type == "cc").ToList().ForEach(r => mail.CC.Add(r.email));

                    if (notification.NotificationRecipients.Count() < 150)
                    {
                        notification.NotificationRecipients.Where(r => r.recipient_type == "bcc").ToList().ForEach(r => mail.Bcc.Add(r.email));
                        smtp.Send(mail);
                    }
                    else
                    {
                        int count = 0;
                        foreach (var recipient in notification.NotificationRecipients.Where(r => r.recipient_type == "bcc"))
                        {
                            mail.Bcc.Add(recipient.email);
                            count++;
                            if (count % 150 == 0)
                            {
                                smtp.Send(mail);
                                emailsent = true;
                                //reset Bcc List
                                mail.Bcc.Clear();
                            }
                        }

                        if (mail.Bcc.Count() > 0)
                        {
                            smtp.Send(mail);
                        }
                    }
                    emailsent = true;

                    NotificationStatus status = db.NotificationStatus.Where(s => s.name == "Sent").SingleOrDefault();
                    notification.status_id = status.id;
                    notification.send_time = DateTime.Now;
                    notification.modified = DateTime.Now;
                    notification.modified_by = User.Identity.Name;
                    db.SaveChanges();
                    return notification;
                }
                catch (Exception e)
                {
                    if (emailsent)
                    {
                        Session["FlashMessage"] += "<br/><br/>Notifications sent with error. <br/><br/>" + e.Message;
                        NotificationStatus status = db.NotificationStatus.Where(s => s.name == "Error").SingleOrDefault();
                        notification.status_id = status.id;
                        notification.send_time = DateTime.Now;
                        notification.modified = DateTime.Now;
                        notification.modified_by = User.Identity.Name;
                        return notification;
                    }
                    else
                    {
                        Session["FlashMessage"] += "<br/><br/>Failed to send notifications. <br/><br/>" + e.Message;
                    }
                }
            }
            return notification;
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
