using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace BlazorServerApp.Models
{
    public class Project
    {
        [Key]
        public int ProjectID { get; set; }

        [Required]
        public string UserID { get; set; }

        [ForeignKey("UserID")]
        public virtual IdentityUser User { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string Language { get; set; }
        public string Description { get; set; }

        [Required]
        public DateTime CreationDate { get; set; } = DateTime.UtcNow;
    }
}
