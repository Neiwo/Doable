using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Doable.Models
{
    public class Tasklist
    {
        [Key]
        public int ID { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        [StringLength(50)]
        public string CreatedBy { get; set; }

        [StringLength(50)]
        public string AssignedTo { get; set; }

        [Required]
        [StringLength(50)]
        public string Priority { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? Deadline { get; set; }

        public ICollection<Notes> Notes { get; set; } = new List<Notes>();

        public ICollection<Docu> Docus { get; set; } = new List<Docu>();

        // New property to store members
        public ICollection<Member> Members { get; set; } = new List<Member>();
    }

    public class Member
    {
        [Key]
        public int ID { get; set; }

        public int TasklistID { get; set; }

        public Tasklist Tasklist { get; set; }

        [StringLength(50)]
        public string Username { get; set; }
    }
}
