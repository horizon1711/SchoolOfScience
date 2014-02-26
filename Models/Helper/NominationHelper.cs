using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NominationHelper))]
    public partial class Nomination { }

    public class NominationHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Program")]
        public int program_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nominator")]
        public int nominator_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Nomination Status")]
        public int status_id { get; set; }

        [Display(Name = "Quota")]
        public Nullable<int> quota { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public System.DateTime start_date { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public System.DateTime end_date { get; set; }
    }
}