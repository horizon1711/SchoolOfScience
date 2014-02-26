using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;

namespace SchoolOfScience.Models.ViewModels
{
    public class ProgramViewModel
    {
        public ProgramViewModel()
        {
            this.no_of_attachments = 5;
            this.no_of_options = 10;
            this.no_of_app_attachments = 5;
        }

        public ProgramViewModel(int no_of_attachments = 5, int no_of_options = 10, int no_of_app_attachments = 5)
        {
            this.no_of_attachments = no_of_attachments;
            this.no_of_options = no_of_options;
            this.no_of_app_attachments = no_of_app_attachments;
        }

        public Program program { get; set; }
        public IList<ProgramAttachment> attachments { get; set; }
        public IList<ProgramOptionValue> options { get; set; }
        public IList<ProgramApplicationAttachment> app_attachments { get; set; }
        public int no_of_attachments { get; set; }
        public int no_of_options { get; set; }
        public int no_of_app_attachments { get; set; }
    }
}