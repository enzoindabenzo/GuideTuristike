using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using DK1.Models.Cal;
using DK1.Models;

namespace DK1.Models.Cal
{
    public class Location
    {
            public int Id { get; set; }

            [Required(ErrorMessage = "Location name is required")]
            [Display(Name = "Location Name")]
            public string Name { get; set; }

            [Display(Name = "Latitude")]
            public double Latitude { get; set; }

            [Display(Name = "Longitude")]
            public double Longitude { get; set; }

            // You can add additional properties specific to Albanian locations if needed
            [Display(Name = "Region")]
            public string Region { get; set; }

            [Display(Name = "Is Popular Destination")]
            public bool IsPopularDestination { get; set; }
    }
}
