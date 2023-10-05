using System.ComponentModel.DataAnnotations;

namespace Drivers.Api.Models.Dtos;


public class UserLoginRequestDto
{
    [Required]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string Password { get; set; } = string.Empty;
}