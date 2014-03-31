using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolOfScience.Models.ViewModels
{
    public class AppointmentCreateMultipleViewModel
    {
        public AppointmentCreateMultipleViewModel()
        {
        }

        public TimeslotConfig config { get; set; }
        public IList<TimeslotConfigSession> sessions { get; set; }
        public IList<Timeslot> timeslots { get; set; }
        public IList<DateTime> skipped_dates { get; set; }
        public Appointment appointment { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Duration")]
        [Range(1, 180, ErrorMessage = "Out of Range. Must be 1 to 180.")]
        public int duration { get; set; }
        public int[] concerns { get; set; }
    }
}