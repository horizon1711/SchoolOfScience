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
    
    public partial class Timeslot
    {
        public int id { get; set; }
        public int timeslot_config_id { get; set; }
        public System.DateTime start_time { get; set; }
        public System.DateTime end_time { get; set; }
    
        public virtual TimeslotConfig TimeslotConfig { get; set; }
    }
}