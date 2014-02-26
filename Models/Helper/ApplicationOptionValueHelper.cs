using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ApplicationOptionValueHelper))]
    public partial class ApplicationOptionValue { }

    public class ApplicationOptionValueHelper
    {

        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        public string value { get; set; }
    }
}