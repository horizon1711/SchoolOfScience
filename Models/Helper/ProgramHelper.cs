using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ProgramHelper))]
    public partial class Program { }

    public class ProgramHelper
    {
        [Display(Name = "Program Type")]
        public int type_id;

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Program Name")]
        public string name;

        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Program Description")]
        public string description;

        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Program Period")]
        public string start_time;

        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Program End Time")]
        public string end_time;

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Application Start Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        [DataType(DataType.DateTime)]
        public Nullable<System.DateTime> application_start_time;

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Application End Time")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        [DataType(DataType.DateTime)]
        public Nullable<System.DateTime> application_end_time;

        [Display(Name = "Deadline Reminder Date")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
        [DataType(DataType.DateTime)]
        public Nullable<System.DateTime> deadline_reminder_date;

        [Display(Name = "Status")]
        public int status_id;

        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Host Name")]
        public string host_name;

        [Url]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Website")]
        public string website;

        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Job Position")]
        public string job_position;

        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Enquiry")]
        public string enquiry;

        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Student Type")]
        public string eligible_academic_career;

        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Department")]
        public string eligible_academic_organization;

        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Academic Plan")]
        public string eligible_academic_plan;

        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Academic Level")]
        public string eligible_academic_level;

        [StringLength(100, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Program Status")]
        public string eligible_program_status;

        [Display(Name = "Interview Required")]
        public bool require_interview;

        [Display(Name = "Nomination Required")]
        public bool require_nomination;

        [Display(Name = "Consultation Required")]
        public bool require_appointment;

        [Display(Name = "Desired Exchange Destinations Required")]
        public bool require_exchange_option;

        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Eligibility")]
        public string eligibility;

        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Notes for Exchange Program")]
        public string note_exchange;

        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Special Criteria (for Nominators)")]
        public string special_criteria;

        [Display(Name = "Vacancies")]
        public Nullable<int> vacancies;

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Apply Action")]
        public string apply_action;

        [Display(Name = "Apply link/email")]
        public string apply_link;

        [Display(Name = "Created By")]
        public string created_by;

        [Display(Name = "Modified By")]
        public string modified_by;

        [Display(Name = "Created")]
        public System.DateTime created;

        [Display(Name = "Modified")]
        public System.DateTime modified;

    }

    public class ProgramAction
    {
        public Program program { get; set; }
        public StudentProfile student { get; set; }
        public Application application { get; set; }
        public Interview interview { get; set; }
        public Appointment appointment { get; set; }
        public bool eligible { get; set; }
        public bool inperiod { get; set; }
        public bool beforestart { get; set; }
        public bool existed { get; set; }
        public bool saved { get; set; }
        public bool open { get; set; }
    }
}