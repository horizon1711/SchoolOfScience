using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(TimeslotConfigHelper))]
    public partial class TimeslotConfig { }

    public class TimeslotConfigHelper
    {

        [Display(Name = "Timeslot Name")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public System.DateTime start_date { get; set; }

        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public System.DateTime end_date { get; set; }

        [Display(Name = "Monday")]
        public bool monday { get; set; }

        [Display(Name = "Tuesday")]
        public bool tuesday { get; set; }

        [Display(Name = "Wednesday")]
        public bool wednesday { get; set; }

        [Display(Name = "Thursday")]
        public bool thursday { get; set; }

        [Display(Name = "Friday")]
        public bool friday { get; set; }

        [Display(Name = "Saturday")]
        public bool saturday { get; set; }

        [Display(Name = "Sunday")]
        public bool sunday { get; set; }

        [Display(Name = "Public Holiday")]
        public bool holiday { get; set; }
    }
}