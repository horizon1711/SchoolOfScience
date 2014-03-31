using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NominationStatusHelper))]
    public partial class NominationStatus { }

    public class NominationStatusHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nomination Status")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Shortlisted")]
        public bool shortlisted { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Open for Nomination")]
        public bool nominated { get; set; }
    }
}