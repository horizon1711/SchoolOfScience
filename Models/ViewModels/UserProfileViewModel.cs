using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SchoolOfScience.Models;
using System.ComponentModel.DataAnnotations;

namespace SchoolOfScience.Models.ViewModels
{
    public class UserProfileViewModel
    {
        public UserProfileViewModel()
        {
        }
        
        public UserProfile profile { get; set; }
        [Display(Name = "User Role")]
        public int? role_id { get; set; }
    }
}