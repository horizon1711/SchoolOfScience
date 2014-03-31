using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;

namespace SchoolOfScience.Models.ViewModels
{
    public class NominationViewModel
    {
        public Nomination nomination { get; set; }
        public Program program { get; set; }
        public string applicationids { get; set; }
    }
}