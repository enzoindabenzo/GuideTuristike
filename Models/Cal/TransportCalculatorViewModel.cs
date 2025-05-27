using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace DK1.Models.Cal
{
    public class TransportCalculatorViewModel { 
    [Required(ErrorMessage = "Please select origin location")]
    [Display(Name = "Origin")]
    public int OriginId { get; set; }

    [Required(ErrorMessage = "Please select destination location")]
    [Display(Name = "Destination")]
    public int DestinationId { get; set; }

    [Required(ErrorMessage = "Please select transport type")]
    [Display(Name = "Transport Type")]
    public TransportType TransportType { get; set; }

    // Properties for displaying results
    public double Distance { get; set; } // in kilometers
    public double Cost { get; set; } // in Albanian Lek

    // For dropdown lists in the view
    public IEnumerable<Location> Locations { get; set; }

    // Selected location objects (for display purposes)
    public Location Origin { get; set; }
    public Location Destination { get; set; }
}
}