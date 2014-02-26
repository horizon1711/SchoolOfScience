//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SchoolOfScience.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class NotificationTemplate
    {
        public NotificationTemplate()
        {
            this.Notifications = new HashSet<Notification>();
            this.NotificationTypes = new HashSet<NotificationType>();
        }
    
        public int id { get; set; }
        public int type_id { get; set; }
        public string name { get; set; }
        public string sender { get; set; }
        public string cc { get; set; }
        public string bcc { get; set; }
        public string subject { get; set; }
        public string body { get; set; }
    
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual NotificationType NotificationType { get; set; }
        public virtual ICollection<NotificationType> NotificationTypes { get; set; }
    }
}