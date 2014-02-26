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
        [Display(Name = "Application Status")]
        public string name { get; set; }
    }
}