using System;
using System.ComponentModel.DataAnnotations;

namespace Doable.Models
{
    public class Docu
    {
        [Key]
        public int ID { get; set; }

        [Required]
        public int TasklistID { get; set; } // Foreign key to Tasklist

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        [Required]
        public string FilePath { get; set; } // Path where the file is stored

        [Required]
        public DateTime UploadedDate { get; set; }

        public string Uploadedby { get; set; }

        // Navigation property
        public Tasklist Tasklist { get; set; }
    }
}
