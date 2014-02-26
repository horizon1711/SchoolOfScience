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
        
        [Required]
        [Range(1, 180, ErrorMessage = "Duration must be between 1 and 180.")]
        public int duration { get; set; }
        public int concern { get; set; }
    }
}