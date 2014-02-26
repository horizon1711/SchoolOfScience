using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentActivityHelper))]
    public partial class StudentActivity { }

    public class StudentActivityHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Student")]
        public string student_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public System.DateTime start_date { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public System.DateTime end_date { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Activity Name")]
        public string name { get; set; }

        [Display(Name = "Data Source")]
        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        public string data_source { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        public string remarks { get; set; }

    }
}