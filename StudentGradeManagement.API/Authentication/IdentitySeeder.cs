
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StudentGradeManagement.API.Data;

namespace StudentGradeManagement.API.Authentication;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(
        IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        string[] roles = { "Student", "Faculty" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        $"Failed to create role {role}: {errors}");
                }
            }
        }
    }

    public static async Task SeedFacultyAccountAsync(
        IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var academicDb = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        // Development-only demo account.
        const string email = "faculty.demo@example.com";

        var password = Environment.GetEnvironmentVariable(
            "SGMS_FACULTY_DEMO_PASSWORD");

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "SGMS_FACULTY_DEMO_PASSWORD environment variable is not configured.");
        }

        // Choose an active faculty record that has an email.
        var faculty = await academicDb.Faculty
            .Where(f => f.IsActive && f.Email != null)
            .OrderBy(f => f.FacultyId)
            .FirstOrDefaultAsync();

        if (faculty == null)
        {
            throw new InvalidOperationException(
                "No active faculty record with an email was found.");
        }

        var roleExists = await roleManager.RoleExistsAsync("Faculty");

        if (!roleExists)
        {
            throw new InvalidOperationException(
                "Faculty role does not exist. Seed roles first.");
        }

        var user = await userManager.FindByEmailAsync(email);

        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false,
                FacultyId = faculty.FacultyId
            };

            var createResult = await userManager.CreateAsync(
                user, password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    createResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to create faculty account: {errors}");
            }
        }
        else if (user.FacultyId != faculty.FacultyId)
        {
            throw new InvalidOperationException(
                "The existing demo account is linked to a different faculty record.");
        }

        if (!await userManager.IsInRoleAsync(user, "Faculty"))
        {
            var roleResult = await userManager.AddToRoleAsync(
                user, "Faculty");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(
                    "; ",
                    roleResult.Errors.Select(e => e.Description));

                throw new InvalidOperationException(
                    $"Failed to assign Faculty role: {errors}");
            }
        }
    }
}