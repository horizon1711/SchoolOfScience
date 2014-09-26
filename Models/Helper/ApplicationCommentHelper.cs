using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ApplicationCommentHelper))]
    public partial class ApplicationComment { }

    public class ApplicationCommentHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Application ID")]
        public int application_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Comment")]
        public string comment { get; set; }

        [Display(Name = "Created By")]
        public string created_by { get; set; }

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime created { get; set; }

        [Display(Name = "Modified By")]
        public string modified_by { get; set; }

        [Display(Name = "Modified")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime modified { get; set; }
    }
}