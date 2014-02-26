using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolOfScience.Models.ViewModels
{
    public class StudentExperienceViewModel
    {
        public StudentExperienceViewModel()
        {
        }

        public IEnumerable<StudentExperienceType> types { get; set; }
        public IEnumerable<StudentExperience> experiences { get; set; }

        public string student_id { get; set; }
        public int type_id { get; set; }
        public string organization { get; set; }
        public string position { get; set; }
        public string duty_description { get; set; }
    }
}