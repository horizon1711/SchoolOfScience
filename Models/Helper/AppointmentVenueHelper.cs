using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(AppointmentVenueHelper))]
    public partial class AppointmentVenue { }

    public class AppointmentVenueHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Venue")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Status")]
        public bool status { get; set; }
    }
}