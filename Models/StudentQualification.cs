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
    
    public partial class StudentQualification
    {
        public int id { get; set; }
        public string student_id { get; set; }
        public string organization { get; set; }
        public Nullable<System.DateTime> award_date { get; set; }
        public string name { get; set; }
    
        public virtual StudentProfile StudentProfile { get; set; }
    }
}
