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
    
    public partial class NominationStatus
    {
        public NominationStatus()
        {
            this.Nominations = new HashSet<Nomination>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public bool shortlisted { get; set; }
        public bool nominated { get; set; }
    
        public virtual ICollection<Nomination> Nominations { get; set; }
    }
}
