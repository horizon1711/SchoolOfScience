using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ApplicationStatusHelper))]
    public partial class ApplicationStatus { }

    public class ApplicationStatusHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Application Status")]
        public string name { get; set; }

        [Display(Name = "Lock")]
        public bool locked { get; set; }
        [Display(Name = "Save")]
        public bool editable { get; set; }
        [Display(Name = "Submit")]
        public bool submitted { get; set; }
        [Display(Name = "Default")]
        public bool default_status { get; set; }
        [Display(Name = "Reject")]
        public bool rejected { get; set; }
    }
}