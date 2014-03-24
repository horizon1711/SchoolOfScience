using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(InterviewStatusHelper))]
    public partial class InterviewStatus { }

    public class InterviewStatusHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Interview Status")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Display to Student")]
        public bool display_to_student { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Default Status")]
        public bool default_status { get; set; }
    }
}