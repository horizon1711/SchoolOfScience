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
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Displayed to Student")]
        public bool shown_to_student { get; set; }
    }
}