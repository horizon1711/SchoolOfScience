using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(AppointmentHelper))]
    public partial class Appointment { }

    public class AppointmentHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime start_time { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "End Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime end_time { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Host")]
        public int host_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Venue")]
        public string venue { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Status")]
        public int status_id { get; set; }

        [Display(Name = "Student ID")]
        public Nullable<int> student_id { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        public string remarks { get; set; }
    }
}