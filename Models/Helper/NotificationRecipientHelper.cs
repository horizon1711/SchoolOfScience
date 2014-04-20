using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NotificationRecipientHelper))]
    public partial class NotificationRecipient { }

    public class NotificationRecipientHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Notification")]
        public int notification_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Email Address")]
        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        public string email { get; set; }
    }
}