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
    
    public partial class NotificationType
    {
        public NotificationType()
        {
            this.Notifications = new HashSet<Notification>();
            this.NotificationTemplates = new HashSet<NotificationTemplate>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public Nullable<int> template_id { get; set; }
    
        public virtual ICollection<Notification> Notifications { get; set; }
        public virtual ICollection<NotificationTemplate> NotificationTemplates { get; set; }
        public virtual NotificationTemplate NotificationTemplate { get; set; }
    }
}