using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolOfScience.Models.ViewModels
{
    public class ApplicationExportViewModel
    {
        public ApplicationExportViewModel()
        {
        }

        public Program program { get; set; }
        public ICollection<StudentParticularType> particulartypes { get; set; }
        public ICollection<StudentExperienceType> experiencetypes { get; set; }

    }
}