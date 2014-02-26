using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;

namespace SchoolOfScience.Models.ViewModels
{
    public class InterviewCreateMultipleViewModel
    {
        public InterviewCreateMultipleViewModel()
        {
        }

        public TimeslotConfig config { get; set; }
        public IList<TimeslotConfigSession> sessions { get; set; }
        public IList<Timeslot> timeslots { get; set; }
        public IList<DateTime> skipped_dates { get; set; }
        public Interview interview { get; set; }
    }
}