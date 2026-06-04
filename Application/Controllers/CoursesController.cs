using System.Security.Claims;
using Application.Data;
using Application.DTOs.Courses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CoursesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CourseListItemDto>>> GetCourses()
    {
        var courses = await _context.Courses
            .AsNoTracking()
            .Where(x => x.IsPublished)
            .OrderBy(x => x.Title)
            .Select(x => new CourseListItemDto
            {
                Id = x.Id,
                Title = x.Title,
                Slug = x.Slug,
                ShortDescription = x.ShortDescription,
                Level = x.Level.ToString(),
                ThumbnailUrl = x.ThumbnailUrl,
                CategoryName = x.Category != null ? x.Category.Name : string.Empty,
                CategorySlug = x.Category != null ? x.Category.Slug : string.Empty,
                ModuleCount = x.Modules.Count,
                LessonCount = x.Modules.SelectMany(module => module.Lessons).Count(lesson => lesson.IsPublished),
                TotalDurationInSeconds = x.Modules
                    .SelectMany(module => module.Lessons)
                    .Where(lesson => lesson.IsPublished)
                    .Select(lesson => lesson.DurationInSeconds)
                    .Sum()
            })
            .ToListAsync();

        return Ok(courses);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult<CourseDetailsDto>> GetCourseBySlug(string slug)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .Include(x => x.Category)
            .Include(x => x.Modules.OrderBy(module => module.Order))
                .ThenInclude(module => module.Lessons.Where(lesson => lesson.IsPublished).OrderBy(lesson => lesson.Order))
            .FirstOrDefaultAsync(x => x.IsPublished && x.Slug == slug);

        if (course == null)
        {
            return NotFound();
        }

        var modules = course.Modules
            .OrderBy(x => x.Order)
            .Select(module => new CourseModuleDto
            {
                Id = module.Id,
                Title = module.Title,
                Description = module.Description,
                Order = module.Order,
                TotalDurationInSeconds = module.Lessons.Sum(lesson => lesson.DurationInSeconds),
                Lessons = module.Lessons
                    .OrderBy(lesson => lesson.Order)
                    .Select(lesson => new LessonSummaryDto
                    {
                        Id = lesson.Id,
                        Title = lesson.Title,
                        Slug = lesson.Slug,
                        Summary = lesson.Summary,
                        DurationInSeconds = lesson.DurationInSeconds,
                        Order = lesson.Order,
                        IsPreview = lesson.IsPreview
                    })
                    .ToList()
            })
            .ToList();

        var response = new CourseDetailsDto
        {
            Id = course.Id,
            Title = course.Title,
            Slug = course.Slug,
            ShortDescription = course.ShortDescription,
            Description = course.Description,
            Level = course.Level.ToString(),
            ThumbnailUrl = course.ThumbnailUrl,
            CategoryName = course.Category?.Name ?? string.Empty,
            CategorySlug = course.Category?.Slug ?? string.Empty,
            TotalDurationInSeconds = modules.Sum(x => x.TotalDurationInSeconds),
            ModuleCount = modules.Count,
            LessonCount = modules.Sum(x => x.Lessons.Count),
            Modules = modules
        };

        return Ok(response);
    }

    [HttpGet("{courseSlug}/lessons/{lessonSlug}")]
    public async Task<ActionResult<LessonDetailsDto>> GetLessonBySlug(string courseSlug, string lessonSlug)
    {
        var lesson = await _context.Lessons
            .AsNoTracking()
            .Include(x => x.CourseModule)
                .ThenInclude(module => module!.Course)
            .FirstOrDefaultAsync(x =>
                x.IsPublished &&
                x.Slug == lessonSlug &&
                x.CourseModule != null &&
                x.CourseModule.Course != null &&
                x.CourseModule.Course.IsPublished &&
                x.CourseModule.Course.Slug == courseSlug);

        if (lesson == null)
        {
            return NotFound();
        }

        var response = new LessonDetailsDto
        {
            Id = lesson.Id,
            CourseId = lesson.CourseModule!.Course!.Id,
            CourseTitle = lesson.CourseModule.Course.Title,
            CourseSlug = lesson.CourseModule.Course.Slug,
            ModuleId = lesson.CourseModule.Id,
            ModuleTitle = lesson.CourseModule.Title,
            Title = lesson.Title,
            Slug = lesson.Slug,
            Summary = lesson.Summary,
            Content = lesson.Content,
            VideoUrl = lesson.VideoUrl,
            DurationInSeconds = lesson.DurationInSeconds,
            Order = lesson.Order,
            IsPreview = lesson.IsPreview
        };

        return Ok(response);
    }

    [HttpPost("{courseId}/enroll")]
    [Authorize]
    public async Task<ActionResult> EnrollInCourse(int courseId)
    {
        var userId = GetUserId();

        var course = await _context.Courses.FindAsync(courseId);
        if (course == null || !course.IsPublished)
        {
            return NotFound("Curso não encontrado");
        }

        var existingEnrollment = await _context.Enrollments
            .AnyAsync(x => x.CourseId == courseId && x.UserId == userId);

        if (existingEnrollment)
        {
            return BadRequest("Usuário já está matriculado neste curso");
        }

        var enrollment = new Models.Enrollment
        {
            CourseId = courseId,
            UserId = userId,
            EnrolledAt = DateTime.UtcNow,
            Status = Models.EnrollmentStatus.Active
        };

        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();

        return Ok(new { enrollment.Id, enrollment.CourseId, enrollment.UserId, enrollment.EnrolledAt });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
