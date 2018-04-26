using System;
using System.ComponentModel.DataAnnotations;

namespace Hitchhikers.Models
{
    public class CreateViewModel
    {
    
        [Required]
        [Display(Name = "Photo")]
        public string PictName {get; set;}

        [Required]
        [Display(Name = "States")]
        public string States {get; set;}
        
        [Required]
        [Display(Name = "City")]
        public string City {get; set;}

        [Required]
        [Display(Name = "Description")]
        public string Description {get; set;}

        [Display(Name= "Address(optional)")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Date of Visit is required")]
        [Display(Name = "Date of Visit")]
        public DateTime DateOfVisit {get; set;}
    }
}