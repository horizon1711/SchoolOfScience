using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ProgramStatusHelper))]
    public partial class ProgramStatus { }

    public class ProgramStatusHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Program Status")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        public string name { get; set; }

        [Display(Name = "Draft")]
        public bool draft { get; set; }
        [Display(Name = "Display to Others")]
        public bool shown_to_student { get; set; }
        [Display(Name = "Lock")]
        public bool locked { get; set; }
        [Display(Name = "Open for Application")]
        public bool open_for_application { get; set; }
        [Display(Name = "Default")]
        public bool default_status { get; set; }
    }
}