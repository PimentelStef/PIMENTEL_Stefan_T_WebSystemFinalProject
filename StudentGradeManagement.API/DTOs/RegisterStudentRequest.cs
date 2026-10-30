using System.ComponentModel.DataAnnotations;

namespace StudentGradeManagement.API.DTOs;

public class RegisterStudentRequest
{
    [Required]
    public string StudentNumber { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;
}