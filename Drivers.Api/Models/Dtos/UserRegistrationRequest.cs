using System.ComponentModel.DataAnnotations;

namespace Drivers.Api.Models.Dtos;


public class UserRegistrationRequest
{
    [Required]
    public string Name { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}
