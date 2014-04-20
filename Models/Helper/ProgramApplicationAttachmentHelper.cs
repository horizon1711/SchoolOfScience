using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ProgramApplicationAttachmentHelper))]
    public partial class ProgramApplicationAttachment { }

    public class ProgramApplicationAttachmentHelper
    {
        [Display(Name = "Application Attachment")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        public string name { get; set; }

        [Display(Name = "Required")]
        public bool required { get; set; }
    }
}