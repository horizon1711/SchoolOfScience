using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ProgramTypeHelper))]
    public partial class ProgramType { }

    public class ProgramTypeHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Program Type")]
        public string name { get; set; }

        [Display(Name = "Program Type Shortname")]
        [StringLength(5, ErrorMessage = "*Maximum length exceeded.")]
        public string shortname { get; set; }

        [Display(Name = "Display Date Type")]
        public string display_date { get; set; }

        [Display(Name = "Display on Menu")]
        public bool display_on_menu { get; set; }

        [Display(Name = "Display on Showcase")]
        public bool display_on_showcase { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Range(0, 1000, ErrorMessage = "Duration must be between 0 and 1000.")]
        [Display(Name = "Display priority")]
        public int priority { get; set; }

    }
}