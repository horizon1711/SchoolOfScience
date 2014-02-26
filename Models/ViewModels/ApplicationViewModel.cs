using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;

namespace SchoolOfScience.Models.ViewModels
{
    public class ApplicationViewModel
    {
        public ApplicationViewModel()
        {
            this.no_of_exchange_options = 5;
        }

        public Program program { get; set; }
        public Application application { get; set; }
        public StudentProfile student { get; set; }
        public IList<ApplicationOptionValue> options { get; set; }
        public IList<ApplicationExchangeOption> exchange_options { get; set; }
        public IList<ApplicationAttachment> attachments { get; set; }
        public int? interview_id { get; set; }
        public int no_of_exchange_options { get; set; }
    }
}