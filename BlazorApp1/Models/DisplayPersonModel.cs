using System.ComponentModel.DataAnnotations;

namespace BlazorApp1.Models
{
    public class DisplayPersonModel
    {
        [Required]
        [StringLength(15, ErrorMessage = "First Name is too long.")]
        [MinLength(5, ErrorMessage = "First Name is too short.")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(15, ErrorMessage = "Last Name is too long.")]
        [MinLength(5, ErrorMessage = "Last Name is too short.")]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }
    }
}
