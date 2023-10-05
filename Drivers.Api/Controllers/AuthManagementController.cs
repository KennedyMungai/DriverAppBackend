using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Drivers.Api.Configurations;
using Drivers.Api.Models.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Drivers.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthManagementController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly JwtConfig _jwtConfig;

    public AuthManagementController(
        UserManager<IdentityUser> userManager,
        IOptionsMonitor<JwtConfig> _optionsMonitor
    )
    {
        _userManager = userManager;
        _jwtConfig = _optionsMonitor.CurrentValue;
    }

    [HttpPost("Register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest requestDto)
    {
        if (!ModelState.IsValid)
        {
            return await Task.FromResult(BadRequest("Invalid Request Payload"));
        }

        var emailExist = await _userManager.FindByEmailAsync(requestDto.Email);

        if (emailExist != null)
        {
            return await Task.FromResult(BadRequest("Email already exists"));
        }

        var newUser = new IdentityUser()
        {
            UserName = requestDto.Email,
            Email = requestDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(requestDto.Password)
        };

        var isCreated = await _userManager.CreateAsync(newUser);

        if (isCreated.Succeeded)
        {
            var token = GenerateJwtToken(newUser);

            return await Task.FromResult(Ok(new RegistrationRequestResponse()
            {
                Result = true,
                Token = token
            }));
        }

        return await Task.FromResult(BadRequest(isCreated.Errors.Select(x => x.Description).ToList()));
    }

    [HttpPost("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login([FromBody] UserLoginRequestDto requestDto)
    {
        if (!ModelState.IsValid)
        {
            return await Task.FromResult(BadRequest("Invalid Request Payload"));
        }

        var existingUser = await _userManager.FindByEmailAsync(requestDto.Email);

        if (existingUser is null && !BCrypt.Net.BCrypt.Verify(requestDto.Password, existingUser?.PasswordHash))
        {
            return await Task.FromResult(Forbid("Invalid Credentials"));
        }

        var token = GenerateJwtToken(existingUser);

        return await Task.FromResult(Ok(new LoginRequestResponse()
        {
            Token = token,
            Result = true
        }));
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var jwtTokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor()
        {
            Subject = new ClaimsIdentity(new[] {
                new Claim("Id", user.Id),
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(4),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512)
        };

        var token = jwtTokenHandler.CreateToken(tokenDescriptor);
        var jwtToken = jwtTokenHandler.WriteToken(token);

        return jwtToken;
    }
}