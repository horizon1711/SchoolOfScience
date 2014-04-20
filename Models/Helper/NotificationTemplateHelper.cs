using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NotificationTemplateHelper))]
    public partial class NotificationTemplate { }

    public class NotificationTemplateHelper
    {

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Type")]
        public int type_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Template Name")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Sender")]
        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        public string sender { get; set; }

        [Display(Name = "CC")]
        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        public string cc { get; set; }

        [Display(Name = "BCC(TO for MassMail)")]
        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        public string bcc { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Subject")]
        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        public string subject { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Content")]
        public string body { get; set; }
    }
}