using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(AppointmentHostHelper))]
    public partial class AppointmentHost { }

    public class AppointmentHostHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Host")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Available for Booking")]
        public bool booking { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Advisor/Mentor")]
        public bool advisor { get; set; }
    }
}