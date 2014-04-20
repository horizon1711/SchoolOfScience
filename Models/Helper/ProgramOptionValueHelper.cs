using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ProgramOptionValueHelper))]
    public partial class ProgramOptionValue { }

    public class ProgramOptionValueHelper
    {
        [Display(Name = "Option Value")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        public string name { get; set; }

        [Display(Name = "Required")]
        public bool required { get; set; }
    }
}