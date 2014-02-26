using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolOfScience.Models.ViewModels
{
    public class EmailViewModel
    {
        public EmailViewModel()
        {
        }
        
        [Display(Name = "Recipient")]
        public string to { get; set; }

        [Display(Name = "Sender")]
        public string from { get; set; }

        [Display(Name = "Email Subject")]
        public string subject { get; set; }

        [Display(Name = "Email Content")]
        public string body { get; set; }

        [Display(Name = "SMTP Host")]
        public string host { get; set; }

        [Display(Name = "Port")]
        public int port { get; set; }

        [Display(Name = "Username")]
        public string username { get; set; }

        [Display(Name = "Password")]
        public string password { get; set; }
    }
}