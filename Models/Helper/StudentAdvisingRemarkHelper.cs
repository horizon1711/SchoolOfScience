using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentAdvisingRemarkHelper))]
    public partial class StudentAdvisingRemark { }

    public class StudentAdvisingRemarkHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Student")]
        public string student_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Remarks")]
        public string text { get; set; }

        [Display(Name = "Attachment")]
        public string filename { get; set; }
        public string filepath { get; set; }

        [Display(Name = "Private")]
        public bool @private { get; set; }

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime created { get; set; }
        [Display(Name = "Created By")]
        public string created_by { get; set; }
        [Display(Name = "Modified")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime modified { get; set; }
        [Display(Name = "Modified By")]
        public string modified_by { get; set; }

        [Display(Name = "Date")]
        [Required(ErrorMessage = "*Required Field.")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public System.DateTime display_date { get; set; }
    }
}