using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;

namespace SchoolOfScience.Models.ViewModels
{
    public class StudentProfileViewModel
    {
        public StudentProfileViewModel()
        {
        }

        public StudentProfile student { get; set; }
        public IList<ProgramAction> programactions { get; set; }
        public IList<StudentParticularType> particulartypes { get; set; }
        public IList<StudentExperienceType> experiencetypes { get; set; }
    }
}