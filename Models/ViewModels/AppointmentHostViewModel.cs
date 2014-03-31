using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;

namespace SchoolOfScience.Models.ViewModels
{
    public class AppointmentHostViewModel
    {
        public AppointmentHostViewModel()
        {
        }

        public AppointmentHost host { get; set; }
        public IList<int> userids { get; set; }
    }
}