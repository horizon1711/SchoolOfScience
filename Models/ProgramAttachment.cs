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
    
    public partial class ProgramAttachment
    {
        public int id { get; set; }
        public int program_id { get; set; }
        public string name { get; set; }
        public string filename { get; set; }
        public string filepath { get; set; }
    
        public virtual Program Program { get; set; }
    }
}
