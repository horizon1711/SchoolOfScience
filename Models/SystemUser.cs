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
    
    public partial class SystemUser
    {
        public SystemUser()
        {
            this.UserRoles = new HashSet<SystemRole>();
        }
    
        public int UserId { get; set; }
        public string UserName { get; set; }
        public Nullable<int> appointment_host_id { get; set; }
    
        public virtual AppointmentHost AppointmentHost { get; set; }
        public virtual ICollection<SystemRole> UserRoles { get; set; }
        public virtual webpages_Membership webpages_Membership { get; set; }
    }
}
