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
    
    public partial class Nomination
    {
        public int id { get; set; }
        public int program_id { get; set; }
        public int nominator_id { get; set; }
        public int status_id { get; set; }
        public Nullable<int> quota { get; set; }
        public System.DateTime start_date { get; set; }
        public System.DateTime end_date { get; set; }
    
        public virtual NominationStatus NominationStatus { get; set; }
        public virtual Program Program { get; set; }
    }
}
