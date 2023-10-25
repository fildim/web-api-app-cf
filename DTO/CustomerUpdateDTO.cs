using System.ComponentModel.DataAnnotations;

namespace WebApiApp.DTO
{
    public class CustomerUpdateDTO : BaseDTO
    {
        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(10, ErrorMessage = "The {0} field must be maximum of {1} characters")]
        [RegularExpression(@"^[^\s]+$", ErrorMessage = "Spaces are not allowed")]
        public string? PhoneNo { get; set; }

        [Required(ErrorMessage = "The {0} field is required")]
        [StringLength(100, ErrorMessage = "The {0} field must be maximum of {1} characters")]
        public string? Address { get; set; }
    }
}
