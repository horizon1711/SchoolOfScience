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
    
    public partial class ExchangeOption
    {
        public ExchangeOption()
        {
            this.ApplicationExchangeOptions = new HashSet<ApplicationExchangeOption>();
        }
    
        public int id { get; set; }
        public string name { get; set; }
        public string country { get; set; }
        public string shortcode { get; set; }
        public string remarks { get; set; }
        public bool status { get; set; }
    
        public virtual ICollection<ApplicationExchangeOption> ApplicationExchangeOptions { get; set; }
    }
}