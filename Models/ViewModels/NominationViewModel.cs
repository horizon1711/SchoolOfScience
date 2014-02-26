using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;

namespace SchoolOfScience.Models.ViewModels
{
    public class NominationViewModel
    {
        public NominationViewModel()
        {
        }

        public Nomination nomination { get; set; }
        public IList<Application> applications { get; set; }
    }
}