using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(InterviewHelper))]
    public partial class Interview { }

    public class InterviewHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Program")]
        public int program_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Start Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        [DataType(DataType.DateTime)]
        public System.DateTime start_time { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "End Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        [DataType(DataType.DateTime)]
        public System.DateTime end_time { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Number of Interviewee")]
        public Nullable<int> no_of_interviewee { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Range(1, 180, ErrorMessage = "Duration must be between 1 and 180.")]
        [Display(Name = "Duration (Min)")]
        public int duration { get; set; }

        [Display(Name = "Buffer (Min)")]
        public int buffer { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Venue")]
        public int venue_id { get; set; }

        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Remarks")]
        public string remarks { get; set; }
    }
}