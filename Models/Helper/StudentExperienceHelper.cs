using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentExperienceHelper))]
    public partial class StudentExperience { }

    public class StudentExperienceHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Student")]
        public string student_id { get; set; }

        [Display(Name = "Experience Type")]
        public int type_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Organization/Program")]
        public string organization { get; set; }

        [Display(Name = "Position")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        public string position { get; set; }

        [Display(Name = "Duties")]
        [StringLength(500, ErrorMessage = "*Maximum length exceeded.")]
        public string duty_description { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Year")]
        public int start_year { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Month")]
        public int start_month { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "End Year")]
        public int end_year { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "End Month")]
        public int end_month { get; set; }

    }
}