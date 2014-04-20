using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(AppointmentStatusHelper))]
    public partial class AppointmentStatus { }

    public class AppointmentStatusHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Status")]
        public string name { get; set; }

        [Display(Name = "Lock")]
        public bool locked { get; set; }
        [Display(Name = "Default")]
        public bool default_status { get; set; }
    }
}