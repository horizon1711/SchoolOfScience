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
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Locked")]
        public string locked { get; set; }
    }
}