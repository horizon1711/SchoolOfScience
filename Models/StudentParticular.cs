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
    
    public partial class StudentParticular
    {
        public int id { get; set; }
        public string student_id { get; set; }
        public int type_id { get; set; }
        public string name { get; set; }
    
        public virtual StudentParticularType StudentParticularType { get; set; }
        public virtual StudentProfile StudentProfile { get; set; }
    }
}
