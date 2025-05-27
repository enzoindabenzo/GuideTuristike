using System;
using System.ComponentModel.DataAnnotations;

namespace DK1.Models
{
    public class ReservationViewModel
    {
        public int DestinationId { get; set; }
        public string DestinationName { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100)]
        [Display(Name = "Full Name")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Reservation date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Reservation Date")]
        public DateTime ReservationDate { get; set; }

        [Required(ErrorMessage = "Number of people is required")]
        [Range(1, 100, ErrorMessage = "Number of people must be between 1 and 100")]
        [Display(Name = "Number of People")]
        public int NumberOfPeople { get; set; }

        [Required(ErrorMessage = "Number of days is required")]
        [Range(1, 30, ErrorMessage = "Number of days must be between 1 and 30")]
        [Display(Name = "Number of Days")]
        public int NumberOfDays { get; set; }

        [StringLength(500)]
        [Display(Name = "Additional Notes")]
        public string Notes { get; set; }

        public decimal? MedianDailyExpenseAL { get; set; }

        public decimal? EstimatedPrice
        {
            get
            {
                if (MedianDailyExpenseAL.HasValue && NumberOfDays > 0)
                {
                    return MedianDailyExpenseAL.Value * NumberOfDays;
                }
                return null;
            }
        }

        public decimal? EstimatedTotalPrice
        {
            get
            {
                if (EstimatedPrice.HasValue && NumberOfPeople > 0)
                {
                    // Apply discount logic (2% per additional person, capped at 20%)
                    int additionalPeople = NumberOfPeople > 1 ? NumberOfPeople - 1 : 0;
                    decimal discountPercent = Math.Min(additionalPeople * 2, 20); // 2% per additional person, up to 20%
                    decimal discount = discountPercent / 100;
                    decimal baseTotal = EstimatedPrice.Value * NumberOfPeople;
                    return baseTotal * (1 - discount);
                }
                return null;
            }
        }

        // New properties for payment
        [Required(ErrorMessage = "Payment method is required")]
        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } // e.g., "PayPal", "Physical"

        [Required(ErrorMessage = "Total cost is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Total cost must be a positive value.")]
        [DataType(DataType.Currency)]
        [Display(Name = "Total Cost")]
        public decimal TotalCost { get; set; } // Total cost in ALL, populated by JavaScript
    }
}