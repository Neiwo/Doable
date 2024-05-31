using System;
using System.ComponentModel.DataAnnotations;

namespace Doable.Models
{
    public class Booking
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string CustomerEmail { get; set; }

        [Required]
        [StringLength(100)]
        public string ServiceType { get; set; }

        [Required]
        public DateTime ServiceDateFrom { get; set; }

        [Required]
        public DateTime ServiceDateTo { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        [Required]
        [StringLength(50)]
        public string Payment { get; set; }

        
    }
}