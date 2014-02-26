using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentParticularHelper))]
    public partial class StudentParticular { }

    public class StudentParticularHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Student")]
        public string student_id { get; set; }

        [Display(Name = "Particular Type")]
        public int type_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Student Particular")]
        public string name { get; set; }

    }
}