using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolOfScience.Models.ViewModels
{
    public class InterviewAssignMultipleViewModel
    {
        public InterviewAssignMultipleViewModel()
        {
        }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Program")]
        public int program_id { get; set; }

        [Display(Name = "Sort by Academic Departments")]
        public bool sort_by_dept { get; set; }

        [Display(Name = "Assign Students from Various Departments in a Single Interview")]
        public bool continuous_assign { get; set; }

        public IList<AvoidedSession> avoided_sessions { get; set; }
    }

    public class AvoidedSession
    {
        public AvoidedSession()
        {
        }

        public string[] academic_organizations { get; set; }
        public string[] academic_plans { get; set; }
        public string[] academic_levels { get; set; }

        [Display(Name = "Monday")]
        public bool monday { get; set; }
        [Display(Name = "Tuesday")]
        public bool tuesday { get; set; }
        [Display(Name = "Wednesday")]
        public bool wednesday { get; set; }
        [Display(Name = "Thursday")]
        public bool thursday { get; set; }
        [Display(Name = "Friday")]
        public bool friday { get; set; }
        [Display(Name = "Saturday")]
        public bool saturday { get; set; }
        [Display(Name = "Sunday")]
        public bool sunday { get; set; }
        [Display(Name = "Public Holiday")]
        public bool holiday { get; set; }


        [Required(ErrorMessage = "*Required Field.")]
        [DataType(DataType.Time)]
        [Display(Name = "Start Time")]
        public DateTime start_time { get; set; }
        [Required(ErrorMessage = "*Required Field.")]
        [DataType(DataType.Time)]
        [Display(Name = "End Time")]
        public DateTime end_time { get; set; }
    }
}