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
    
    public partial class StudentParticularType
    {
        public StudentParticularType()
        {
            this.StudentParticulars = new HashSet<StudentParticular>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public int priority { get; set; }
        public string example { get; set; }
    
        public virtual ICollection<StudentParticular> StudentParticulars { get; set; }
    }
}