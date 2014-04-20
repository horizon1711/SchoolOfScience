using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NominationListHelper))]
    public partial class NominationList { }

    public class NominationListHelper
    {
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Display Name")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nomination")]
        public int nomination_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nominator")]
        public int nominator_userid { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Level")]
        public int nomination_level { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Date")]
        public System.DateTime start_date { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "End Date")]
        public System.DateTime end_date { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Range(0, 100, ErrorMessage = "Out of Range. Must be 0 to 100.")]
        [Display(Name = "Quota (0 = unlimited)")]
        public int quota { get; set; }

        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Remarks")]
        public string remarks { get; set; }
    }
}