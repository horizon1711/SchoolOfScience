using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolOfScience.Models.ViewModels
{
    public class StudentParticularViewModel
    {
        public StudentParticularViewModel()
        {
        }

        public IEnumerable<StudentParticularType> types { get; set; }
        public IEnumerable<StudentParticular> particulars { get; set; }

        public string student_id { get; set; }
        public int type_id { get; set; }

        [Required(ErrorMessage="*Required Field.")]
        public string name { get; set; }

        public string description { get; set; }
    }
}