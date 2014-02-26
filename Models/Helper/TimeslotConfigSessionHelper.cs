using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(TimeslotConfigSessionHelper))]
    public partial class TimeslotConfigSession { }

    public class TimeslotConfigSessionHelper
    {
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public System.DateTime start_time { get; set; }
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public System.DateTime end_time { get; set; }
        [Display(Name = "Excluded")]
        public bool excluded { get; set; }
    }
}