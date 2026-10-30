using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Data;
using StudentGradeManagement.API.DTOs;
using StudentGradeManagement.API.Models;
using StudentGradeManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace StudentGradeManagement.API.Controllers;

[Authorize]
[ApiController]
[Route("api/grades")]
public class GradesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GradesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/grades
    [Authorize(Roles = "Faculty")]
    [HttpGet]
    public async Task<IActionResult> GetGrades()
    {
        var grades = await _context.StudentGrades
            .AsNoTracking()
            .OrderBy(g => g.Student.StudentNumber)
            .ThenBy(g => g.SubjectCode)
            .Select(g => new
            {
                g.GradeId,
                g.StudentId,
                StudentNumber = g.Student.StudentNumber,
                StudentName = g.Student.FirstName + " " +
                              g.Student.LastName,
                g.SubjectCode,
                g.SubjectName,
                g.FacultyId,
                FacultyName = g.Faculty.FirstName + " " +
                              g.Faculty.LastName,
                g.Grade,
                g.Remarks,
                g.Semester,
                g.SchoolYear,
                g.DatePosted,
                g.DateModified
            })
            .ToListAsync();

        return Ok(grades);
    }

    [Authorize(Roles = "Student")]
    [HttpGet("my-grades")]
    public async Task<IActionResult> GetMyGrades()
    {
        var studentIdClaim = User.FindFirst("StudentId")?.Value;

        if (!int.TryParse(studentIdClaim, out var studentId))
        {
            return Forbid();
        }

        var grades = await _context.StudentGrades
            .Where(g => g.StudentId == studentId)
            .ToListAsync();

        return Ok(grades);
    }

    [Authorize(Roles = "Faculty")]
    [HttpPost]
    public async Task<IActionResult> CreateGrade(
    CreateGradeRequest request)
    {
        // Validate that the student exists.
        var student = await _context.Students
            .FirstOrDefaultAsync(s => s.StudentId == request.StudentId);

        if (student == null)
        {
            return BadRequest(new
            {
                message = "The selected student does not exist."
            });
        }

        // Prevent duplicate final grades for the same term.
        var duplicate = await _context.StudentGrades.AnyAsync(g =>
            g.StudentId == request.StudentId &&
            g.SubjectCode == request.SubjectCode &&
            g.Semester == request.Semester &&
            g.SchoolYear == request.SchoolYear);

        if (duplicate)
        {
            return Conflict(new
            {
                message = "A grade already exists for this student, subject, semester, and school year."
            });
        }

        // Compute remarks on the server.
        var gradeValue = request.Grade!.Value;
        var remarks = GradeRemarkService.GetRemark(gradeValue);

        var facultyIdClaim = User.FindFirst("FacultyId")?.Value;

        if (!int.TryParse(facultyIdClaim, out var facultyId))
        {
            return Forbid();
        }

        var faculty = await _context.Faculty
            .FirstOrDefaultAsync(f =>
                f.FacultyId == facultyId && f.IsActive);

        if (faculty == null)
        {
            return Forbid();
        }

        var grade = new StudentGrade
        {
            StudentId = request.StudentId!.Value,
            SubjectCode = request.SubjectCode,
            SubjectName = request.SubjectName,
            FacultyId = faculty.FacultyId,
            Grade = gradeValue,
            Remarks = remarks,
            Semester = request.Semester,
            SchoolYear = request.SchoolYear
        };

        _context.StudentGrades.Add(grade);
        await _context.SaveChangesAsync();

        return StatusCode(201, new
        {
            grade.GradeId,
            grade.StudentId,
            grade.SubjectCode,
            grade.SubjectName,
            grade.FacultyId,
            grade.Grade,
            grade.Remarks,
            grade.Semester,
            grade.SchoolYear,
            grade.DatePosted
        });
    }

    [Authorize(Roles = "Student,Faculty")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetGradeById(int id)
    {
        if (User.IsInRole("Student"))
        {
            var studentIdClaim = User.FindFirst("StudentId")?.Value;

            if (!int.TryParse(studentIdClaim, out var studentId))
            {
                return Forbid();
            }

            var ownsGrade = await _context.StudentGrades
                .AnyAsync(g =>
                    g.GradeId == id &&
                    g.StudentId == studentId);

            if (!ownsGrade)
            {
                return NotFound();
            }
        }

        var grade = await _context.StudentGrades
            .AsNoTracking()
            .Where(g => g.GradeId == id)
            .Select(g => new
            {
                g.GradeId,
                g.StudentId,
                StudentNumber = g.Student.StudentNumber,
                StudentName = g.Student.FirstName + " " + g.Student.LastName,
                g.SubjectCode,
                g.SubjectName,
                g.FacultyId,
                FacultyName = g.Faculty.FirstName + " " + g.Faculty.LastName,
                g.Grade,
                g.Remarks,
                g.Semester,
                g.SchoolYear,
                g.DatePosted,
                g.DateModified
            })
            .FirstOrDefaultAsync();

        if (grade == null)
        {
            return NotFound(new { message = "Grade record not found." });
        }

        return Ok(grade);
    }

    [Authorize(Roles = "Faculty")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGrade(
        int id,
        [FromBody] UpdateGradeRequest request)
    {
        var gradeRecord = await _context.StudentGrades
            .FirstOrDefaultAsync(g => g.GradeId == id);

        if (gradeRecord == null)
        {
            return NotFound(new { message = "Grade record not found." });
        }

        var studentExists = await _context.Students
            .AnyAsync(s => s.StudentId == request.StudentId);

        if (!studentExists)
        {
            return BadRequest(new { message = "The specified student does not exist." });
        }

        var duplicateExists = await _context.StudentGrades
            .AnyAsync(g =>
                g.GradeId != id &&
                g.StudentId == request.StudentId &&
                g.SubjectCode == request.SubjectCode &&
                g.Semester == request.Semester &&
                g.SchoolYear == request.SchoolYear);

        if (duplicateExists)
        {
            return Conflict(new
            {
                message = "This student already has a grade for that subject, semester, and school year."
            });
        }

        gradeRecord.StudentId = request.StudentId!.Value;
        gradeRecord.SubjectCode = request.SubjectCode;
        gradeRecord.SubjectName = request.SubjectName;
        gradeRecord.Grade = request.Grade!.Value;
        gradeRecord.Remarks = GradeRemarkService.GetRemark(request.Grade.Value);
        gradeRecord.Semester = request.Semester;
        gradeRecord.SchoolYear = request.SchoolYear;
        gradeRecord.DateModified = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Grade updated successfully.",
            gradeRecord.GradeId,
            gradeRecord.StudentId,
            gradeRecord.SubjectCode,
            gradeRecord.SubjectName,
            gradeRecord.Grade,
            gradeRecord.Remarks,
            gradeRecord.Semester,
            gradeRecord.SchoolYear,
            gradeRecord.DateModified
        });
    }

    [Authorize(Roles = "Faculty")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGrade(int id)
    {
        var gradeRecord = await _context.StudentGrades
            .FirstOrDefaultAsync(g => g.GradeId == id);

        if (gradeRecord == null)
        {
            return NotFound(new { message = "Grade record not found." });
        }

        _context.StudentGrades.Remove(gradeRecord);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}