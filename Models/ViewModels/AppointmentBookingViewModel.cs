using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolOfScience.Models.ViewModels
{
    public class AppointmentBookingViewModel
    {
        public StudentAdvisor advisor { get; set; }

        public IList<AppointmentHost> hostList { get; set; }
        [Required(ErrorMessage = "*Required Field.")]
        public int host_id { get; set; }
        public AppointmentHost host { get; set; }

        public IList<Appointment> appointmentList { get; set; }
        [Required(ErrorMessage = "*Required Field.")]
        public int appointment_id { get; set; }
        public Appointment appointment { get; set; }

        public IList<AppointmentConcern> concernList { get; set; }
        [Required(ErrorMessage = "*Required Field.")]
        public int[] concern_ids { get; set; }
        public IList<AppointmentConcern> concerns { get; set; }
        public string other_concern { get; set; }
    }
}