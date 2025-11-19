using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Real_Estate_Services.Models
{
    public class Sites
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Please enter Site name")]
        public string SiteName { get; set; }

        [Required(ErrorMessage = "Please enter Site Address")]

        public string Category { get; set; }

        [Required(ErrorMessage = "Please enter Site Category")]

        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter Site Description")]
        public string Description { get; set; }

        // Stored image paths
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image3 { get; set; }
        public string? Image4 { get; set; }

        // Not mapped — used only for file uploads
        [NotMapped]
        [Display(Name = "Select up to 7 images")]
        public List<IFormFile>? ImageFiles { get; set; }

        //Brochure
        public string? Brochure { get; set; }

        [NotMapped]
        [Display(Name = "Upload brochure (PDF/DOC)")]
        public IFormFile? BrochureFile { get; set; }
    }
}
