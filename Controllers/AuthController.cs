using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureTaskApi.Auth;
using SecureTaskApi.Data;
using SecureTaskApi.Dtos;
using SecureTaskApi.Models;

namespace SecureTaskApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly JwtTokenService _tokenService;

    public AuthController(AppDbContext context, JwtTokenService tokenService)
    {
        _context = context;
        _tokenService = tokenService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        if (await _context.Users.AnyAsync(u => u.UserName == dto.UserName))
            return BadRequest("Username already taken.");

        var user = new User
        {
            UserName = dto.UserName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User"
        };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok("User registered successfully.");
    }

    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(LoginDto dto)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == dto.UserName);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Invalid credentials.");

        return Ok(_tokenService.GenerateToken(user));
    }
}
