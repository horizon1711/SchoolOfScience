using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentParticularTypeHelper))]
    public partial class StudentParticularType { }

    public class StudentParticularTypeHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Student Particular Type")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Range(0, 1000, ErrorMessage = "Out of Range. Must be 0 to 1000.")]
        [Display(Name = "Priority")]
        public int priority { get; set; }

        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Input Example")]
        public string example { get; set; }

    }
}