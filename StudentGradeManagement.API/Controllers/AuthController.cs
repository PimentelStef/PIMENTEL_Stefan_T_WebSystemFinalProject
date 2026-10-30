using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentGradeManagement.API.Authentication;
using StudentGradeManagement.API.Data;
using StudentGradeManagement.API.DTOs;

namespace StudentGradeManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _db = db;
        _configuration = configuration;
    }

    [HttpPost("register-student")]
    public async Task<IActionResult> RegisterStudent(
        RegisterStudentRequest request)
    {
        var student = await _db.Students
            .FirstOrDefaultAsync(s =>
                s.StudentNumber == request.StudentNumber);

        if (student == null)
        {
            return BadRequest(new
            {
                message = "Student number was not found."
            });
        }

        var existingAccount = await _userManager.Users
            .AnyAsync(u => u.StudentId == student.StudentId);

        if (existingAccount)
        {
            return Conflict(new
            {
                message = "This student already has an account."
            });
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            StudentId = student.StudentId
        };

        var result = await _userManager.CreateAsync(
            user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new
            {
                errors = result.Errors.Select(e => e.Description)
            });
        }

        var roleResult = await _userManager.AddToRoleAsync(
            user, "Student");

        if (!roleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);

            return StatusCode(500, new
            {
                message = "Could not assign the Student role."
            });
        }

        return Ok(new
        {
            message = "Student account created successfully."
        });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null ||
            !await _userManager.CheckPasswordAsync(
                user, request.Password))
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? user.Email ?? "")
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        if (user.StudentId.HasValue)
        {
            claims.Add(new Claim(
                "StudentId", user.StudentId.Value.ToString()));
        }

        if (user.FacultyId.HasValue)
        {
            claims.Add(new Claim(
                "FacultyId", user.FacultyId.Value.ToString()));
        }

        var secretKey = _configuration["Jwt:SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT secret key is missing.");

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler()
                .WriteToken(token),
            expiresAt = token.ValidTo,
            roles
        });
    }
}