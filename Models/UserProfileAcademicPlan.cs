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
    
    public partial class UserProfileAcademicPlan
    {
        public int user_id { get; set; }
        public string academic_plan { get; set; }
    
        public virtual SystemUser UserProfile { get; set; }
    }
}
