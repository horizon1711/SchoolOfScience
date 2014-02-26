using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentQualificationHelper))]
    public partial class StudentQualification { }

    public class StudentQualificationHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Student")]
        public string student_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Name of Awarding Body")]
        public string organization { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Qualification/Award")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Date Obtained")]
        public System.DateTime award_date { get; set; }
    }
}