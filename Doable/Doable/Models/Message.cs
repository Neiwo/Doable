using System;
using System.ComponentModel.DataAnnotations;

namespace Doable.Models
{
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        [Required]
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; }

        public User Sender { get; set; }
        public User Receiver { get; set; }
    }
}
