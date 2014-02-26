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
        [Display(Name = "Exchange Destination Name")]
        public string name { get; set; }

        [Required(ErrorMessage = "*Required Field.")]
        [Display(Name = "Country")]
        public string country { get; set; }

        [Display(Name = "Shortcode")]
        public string shortcode { get; set; }

        [Display(Name = "Remarks")]
        public string remarks { get; set; }

        [Display(Name = "Status")]
        public bool status { get; set; }
    }
}