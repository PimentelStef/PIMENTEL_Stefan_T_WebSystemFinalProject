using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Data;

namespace StudentGradeManagement.API.Controllers;

[ApiController]
[Route("api/database-test")]
public class DatabaseTestController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DatabaseTestController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents()
    {
        var students = await _context.Students
            .AsNoTracking()
            .Select(s => new
            {
                s.StudentId,
                s.StudentNumber,
                s.FirstName,
                s.LastName,
                s.Email
            })
            .ToListAsync();

        return Ok(students);
    }
}