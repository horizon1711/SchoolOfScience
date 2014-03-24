using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ApplicationStatusHelper))]
    public partial class ApplicationStatus { }

    public class ApplicationStatusHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Application Status")]
        public string name { get; set; }
    }
}