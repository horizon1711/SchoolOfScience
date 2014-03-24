using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(ExchangeOptionHelper))]
    public partial class ExchangeOption { }

    public class ExchangeOptionHelper
    {
        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Exchange Destination Name")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        [Display(Name = "Country")]
        public string country { get; set; }

        [Display(Name = "Shortcode")]
        [StringLength(50, ErrorMessage = "*Maximum length exceeded.")]
        public string shortcode { get; set; }

        [Display(Name = "Remarks")]
        [StringLength(200, ErrorMessage = "*Maximum length exceeded.")]
        public string remarks { get; set; }

        [Display(Name = "Status")]
        public bool status { get; set; }
    }
}