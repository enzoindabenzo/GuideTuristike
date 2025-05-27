using System;
using System.ComponentModel.DataAnnotations;

namespace DK1.Models
{
    public class Reservation
    {
        [Key] // Defines Id as the primary key
        public int Id { get; set; }

        [Required]
        public int DestinationId { get; set; } // Foreign Key
        public virtual Destination Destination { get; set; } // Navigation Property

        [Required]
        [StringLength(100)]
        public string UserName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; }

        [Phone]
        [StringLength(20)]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Number of people must be between 1 and 100.")]
        public int NumberOfPeople { get; set; }

        [Range(1, 365, ErrorMessage = "Number of days must be between 1 and 365.")]
        public int NumberOfDays { get; set; } = 1; // Default to 1 day

        [StringLength(500)]
        public string Notes { get; set; }

        [Required]
        public string Status { get; set; } // e.g., "Pending", "Approved", "Rejected"

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // New properties for payment
        [Required] // Required for PayPal transactions, optional for Physical
        [Range(0, double.MaxValue, ErrorMessage = "Total cost must be a positive value.")]
        [DataType(DataType.Currency)]
        public decimal TotalCost { get; set; } // Total cost in ALL

        [Required] // Required to track payment method
        [StringLength(50)]
        public string PaymentMethod { get; set; } // e.g., "PayPal", "Physical"
    }
}