using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NotificationTypeHelper))]
    public partial class NotificationType { }

    public class NotificationTypeHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Type")]
        public string name { get; set; }

        [Display(Name = "Default Template")]
        public Nullable<int> template_id { get; set; }
    }
}