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
    
    public partial class Interview
    {
        public Interview()
        {
            this.Applications = new HashSet<Application>();
            this.InterviewComments = new HashSet<InterviewComment>();
        }
    
        public int id { get; set; }
        public int program_id { get; set; }
        public int status_id { get; set; }
        public System.DateTime start_time { get; set; }
        public System.DateTime end_time { get; set; }
        public Nullable<int> no_of_interviewee { get; set; }
        public int duration { get; set; }
        public int buffer { get; set; }
        public int venue_id { get; set; }
        public string remarks { get; set; }
    
        public virtual InterviewVenue InterviewVenue { get; set; }
        public virtual Program Program { get; set; }
        public virtual ICollection<Application> Applications { get; set; }
        public virtual InterviewStatus InterviewStatus { get; set; }
        public virtual ICollection<InterviewComment> InterviewComments { get; set; }
    }
}
