using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NotificationStatusHelper))]
    public partial class NotificationStatus { }

    public class NotificationStatusHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Status")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        public string name { get; set; }

        [Display(Name = "Default")]
        public bool default_status { get; set; }
        [Display(Name = "Sent")]
        public bool sent { get; set; }
        [Display(Name = "Sent with Error")]
        public bool error { get; set; }
    }
}