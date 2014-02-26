using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(NotificationHelper))]
    public partial class Notification { }

    public class NotificationHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Send Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime send_time { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Sender")]
        public string sender { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Subject")]
        public string subject { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Content")]
        public string body { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Template")]
        public Nullable<int> template_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Status")]
        public int status_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Type")]
        public int type_id { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Created By")]
        public string created_by { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime created { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Modified By")]
        public string modified_by { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Modified")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime modified { get; set; }

        [Display(Name = "Reference Program")]
        public Nullable<int> program_id { get; set; }

        [Display(Name = "Reference Application")]
        public Nullable<int> application_id { get; set; }

        [Display(Name = "Reference Interview")]
        public Nullable<int> interview_id { get; set; }

        [Display(Name = "Reference Appointment")]
        public Nullable<int> appointment_id { get; set; }

        [Display(Name = "Reference Nomination")]
        public Nullable<int> nomination_id { get; set; }
    }
}