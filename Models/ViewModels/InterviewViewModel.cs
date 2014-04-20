using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;

namespace SchoolOfScience.Models.ViewModels
{
    public class InterviewViewModel
    {
        public InterviewViewModel()
        {
        }

        public Interview interview { get; set; }
        public int[] application_ids { get; set; }
    }
}