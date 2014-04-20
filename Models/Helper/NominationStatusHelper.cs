using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NominationStatusHelper))]
    public partial class NominationStatus { }

    public class NominationStatusHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nomination Status")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        public string name { get; set; }
        
        [Display(Name = "Norminated")]
        public bool nominated { get; set; }
        [Display(Name = "Shortlisted")]
        public bool shortlisted { get; set; }
        [Display(Name = "Default")]
        public bool default_status { get; set; }
    }
}