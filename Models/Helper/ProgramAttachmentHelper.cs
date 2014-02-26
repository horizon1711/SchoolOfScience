using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ProgramAttachmentHelper))]
    public partial class ProgramAttachment { }

    public class ProgramAttachmentHelper
    {
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Attachment Title")]
        public string name { get; set; }
    }
}