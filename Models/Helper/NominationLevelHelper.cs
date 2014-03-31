using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NominationLevelHelper))]
    public partial class NominationLevel { }

    public class NominationLevelHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nomination")]
        public int nomination_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public System.DateTime start_date { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public System.DateTime end_date { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Range(0, 1000, ErrorMessage = "Out of Range. Must be 0 to 1000.")]
        [Display(Name = "Quota (0 = no limit)")]
        public int quota { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Range(0, 3, ErrorMessage = "Out of Range. Must be 0 to 3.")]
        [Display(Name = "Nomination Level")]
        public int nomination_level { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        public string remarks { get; set; }
    }
}