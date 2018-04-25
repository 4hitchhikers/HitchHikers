using System;
using System.ComponentModel.DataAnnotations;

namespace Hitchhikers.Models
{
    public class CreateViewModel
    {
        [Required]
        [Display(Name = "Location")]
        public string Location {get; set;}

        [Required]
        [Display(Name = "Photo")]
        public string Photo {get; set;}

        [Required]
        [Display(Name = "Description")]
        public string Description {get; set;}

        [Required]
        [Display(Name = "Date of Visit")]
        public DateTime DateOfVisit {get; set;}
    }
}