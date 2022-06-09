using System.ComponentModel.DataAnnotations;

namespace AspNetCoreTesting.Api.Models
{
    public class AddUserModel
    {
        [Required]
        public string? FirstName { get; set; }
        [Required]
        public string? LastName { get; set; }
    }
}
