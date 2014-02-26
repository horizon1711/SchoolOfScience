using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NominationApplicationHelper))]
    public partial class NominationApplication { }

    public class NominationApplicationHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nomination for Program")]
        public int nomination_id { get; set; }
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Application to Nominate")]
        public int application_id { get; set; }
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Application Nomination Status")]
        public int status_id { get; set; }

    }
}