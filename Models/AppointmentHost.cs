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
    
    public partial class AppointmentHost
    {
        public AppointmentHost()
        {
            this.Appointments = new HashSet<Appointment>();
            this.SystemUsers = new HashSet<SystemUser>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public bool booking { get; set; }
        public bool advisor { get; set; }
    
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<SystemUser> SystemUsers { get; set; }
    }
}
