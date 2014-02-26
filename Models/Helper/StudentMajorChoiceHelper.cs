using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentMajorChoiceHelper))]
    public partial class StudentMajorChoice { }

    public class StudentMajorChoiceHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Student")]
        public string student_id { get; set; }

        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Period")]
        public string period { get; set; }

        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Choice 1")]
        public string choice1 { get; set; }

        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Choice 2")]
        public string choice2 { get; set; }

        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Choice 3")]
        public string choice3 { get; set; }
    }
}