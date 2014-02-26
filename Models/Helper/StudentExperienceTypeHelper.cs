using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentExperienceTypeHelper))]
    public partial class StudentExperienceType { }

    public class StudentExperienceTypeHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Student Experience Type")]
        public string name { get; set; }

    }
}