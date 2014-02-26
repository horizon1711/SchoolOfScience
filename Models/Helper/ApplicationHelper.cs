using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ApplicationHelper))]
    public partial class Application { }

    public class ApplicationHelper
    {

        [Display(Name = "Student Name")]
        public string student_id;
        [Display(Name = "Program Name")]
        public int program_id;
        [Display(Name = "Application Status")]
        public int status_id;

        [Display(Name = "Created By")]
        public string created_by;

        [Display(Name = "Modified By")]
        public string modified_by;

        [Display(Name = "Created")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime created;

        [Display(Name = "Modified")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        public System.DateTime modified;
    }
}