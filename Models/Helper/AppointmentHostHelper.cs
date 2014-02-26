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
        [Display(Name = "Host")]
        public string name{ get; set; }
    }
}