using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DK1.Models
{
    public class Destination
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Region { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }

        public int CategoryId { get; set; }  // Foreign Key
        public virtual Category Category { get; set; }  // Navigation Property

        public decimal? MinDailyExpenseAL { get; set; }  // Minimum daily expense in Albanian Lek
        public decimal? MaxDailyExpenseAL { get; set; }  // Maximum daily expense in Albanian Lek

        // Computed property for displaying the expense range
        public string DailyExpenseRange
        {
            get
            {
                if (MinDailyExpenseAL.HasValue && MaxDailyExpenseAL.HasValue)
                {
                    return $"{MinDailyExpenseAL:N0} - {MaxDailyExpenseAL:N0} ALL";
                }
                else if (MinDailyExpenseAL.HasValue)
                {
                    return $"{MinDailyExpenseAL:N0}+ ALL";
                }
                else if (MaxDailyExpenseAL.HasValue)
                {
                    return $"Up to {MaxDailyExpenseAL:N0} ALL";
                }
                return null;
            }
        }

        // Median daily expense (for display purposes)
        public decimal? MedianDailyExpenseAL
        {
            get
            {
                if (MinDailyExpenseAL.HasValue && MaxDailyExpenseAL.HasValue)
                {
                    return (MinDailyExpenseAL + MaxDailyExpenseAL) / 2;
                }
                return MinDailyExpenseAL ?? MaxDailyExpenseAL;
            }
        }
    }
}
