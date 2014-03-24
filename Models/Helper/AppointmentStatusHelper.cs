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

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Locked")]
        public string locked { get; set; }
    }
}