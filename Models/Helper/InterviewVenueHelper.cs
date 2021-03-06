﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(InterviewVenueHelper))]
    public partial class InterviewVenue { }

    public class InterviewVenueHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Venue")]
        public string name { get; set; }
        
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Status")]
        public bool status { get; set; }
    }
}