using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SchoolOfScience.Models
{
    [MetadataType(typeof(StudentProfileHelper))]
    public partial class StudentProfile { }

    public class StudentProfileHelper
    {
        [Display(Name = "Student ID")]
        public int id;

        [Display(Name = "Student Name")]
        public string name;

        [Display(Name = "Academic Group")]
        public string academic_group;

        [Display(Name = "Academic Organization")]
        public string academic_organization;

        [Display(Name = "Academic Career")]
        public string academic_career;

        [Display(Name = "Academic Program")]
        public string academic_program;

        [Display(Name = "Academic Plan")]
        public string academic_plan_primary;

        [Display(Name = "Academic Plan Description")]
        public string academic_plan_description;

        [Display(Name = "Academic Load")]
        public string academic_load;

        [Display(Name = "Academic Level")]
        public string academic_level;

        [Display(Name = "Student Career Number")]
        public string student_career_number;

        [Display(Name = "Study Form")]
        public string study_form;

        [Display(Name = "Study Form Description")]
        public string study_form_description;

        [Display(Name = "Program Status")]
        public string program_status;

        [Display(Name = "Program Status Description")]
        public string program_status_description;

        [Display(Name = "Study Agreement")]
        public string study_agreement;
        
        [Display(Name = "Chinese Name")]
        public string chinese_name;

        [Display(Name = "HKID")]
        public string hkid;

        [Display(Name = "Passport")]
        public string passport;

        [Display(Name = "Date Of Birth")]
        public System.DateTime date_of_birth;

        [Display(Name = "Gender")]
        public string gender;

        [Display(Name = "Citizenship")]
        public string citizenship;

        [Display(Name = "Citizenship Description")]
        public string citizenship_description;

        [Display(Name = "Residence Code")]
        public string residence_code;

        [Display(Name = "Residence Code Description")]
        public string residence_code_description;

        [Display(Name = "Native Place Code")]
        public string native_place_code;

        [Display(Name = "Native Place Code Description")]
        public string native_place_code_description;

        [Display(Name = "Local or Nonlocal")]
        public string local_or_nonlocal;

        [Display(Name = "Email")]
        public string email;

        [Display(Name = "Dormitory Address 1")]
        public string dormitory_address1;

        [Display(Name = "Dormitory Address 2")]
        public string dormitory_address2;

        [Display(Name = "Mailinng Address 1")]
        public string mailing_address1;

        [Display(Name = "Mailing Address 2")]
        public string mailing_address2;

        [Display(Name = "Mailing Address 3")]
        public string mailing_address3;

        [Display(Name = "Dormitory Address 1")]
        public string mailing_address4;

        [Display(Name = "Mailing Postal Code")]
        public string mailing_postal_code;

        [Display(Name = "Mailing Area Code")]
        public string mailing_area_code;

        [Display(Name = "Mailing Country")]
        public string mailing_country;

        [Display(Name = "Home Address 1")]
        public string home_address1;

        [Display(Name = "Home Address 2")]
        public string home_address2;

        [Display(Name = "Home Address 3")]
        public string home_address3;

        [Display(Name = "Home Address 4")]
        public string home_address4;

        [Display(Name = "Home Postal Code")]
        public string home_postal_code;

        [Display(Name = "Home Area Code")]
        public string home_area_code;

        [Display(Name = "Home Country")]
        public string home_country;

        [Display(Name = "Home Phone Country Code")]
        public string home_phone_country_code;

        [Display(Name = "Home Phone Number")]
        public string home_phone_number;

        [Display(Name = "Contact Phone Country Code")]
        public string contact_phone_country_code;

        [Display(Name = "Contact Phone Number")]
        public string contact_phone_number;

        [Display(Name = "Mobile Phone Country Code")]
        public string mobile_phone_country_code;

        [Display(Name = "Mobile Phone Number")]
        public string mobile_phone_number;

        [Display(Name = "Barcode")]
        public string barcode;

        [Display(Name = "Created By")]
        public string created_by;

        [Display(Name = "Modified By")]
        public string modified_by;

        [Display(Name = "Created")]
        public System.DateTime created;

        [Display(Name = "Modified")]
        public System.DateTime modified;

        [Display(Name = "Remarks")]
        [StringLength(4000, ErrorMessage = "*Maximum length exceeded.")]
        public string remarks;
    }
}