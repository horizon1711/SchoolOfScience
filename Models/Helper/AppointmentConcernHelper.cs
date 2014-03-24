using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(AppointmentConcernHelper))]
    public partial class AppointmentConcern { }

    public class AppointmentConcernHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Concern")]
        public string name { get; set; }

        [Display(Name = "Reference Program")]
        public int? program_id { get; set; }
    }
}