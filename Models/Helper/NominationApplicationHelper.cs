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
        [Display(Name = "Nomination")]
        public int nomination_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nomination List")]
        public int nomination_list_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Application")]
        public int application_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nominated")]
        public bool nominated { get; set; }

        [Display(Name = "Nominate Date")]
        public Nullable<System.DateTime> nominate_date { get; set; }

        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Remarks")]
        public string remarks { get; set; }
    }
}