using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Doable.Models
{
    public class Notes
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int TaskID { get; set; }

        [ForeignKey("TaskID")]
        public Tasklist Tasklist { get; set; }

        [Required]
        [StringLength(1000)]
        public string Remarks { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
