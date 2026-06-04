using System.Security.Claims;
using Application.Data;
using Application.DTOs.Courses;
using Application.DTOs.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly AppDbContext _context;

    public MeController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<MeProfileDto>> GetMe()
    {
        var userId = GetUserId();
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == userId);

        if (user == null)
        {
            return NotFound("Usuário não encontrado");
        }

        return Ok(new MeProfileDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString(),
            CreatedAt = user.CreatedAt
        });
    }

    [HttpGet("courses")]
    public async Task<ActionResult<IReadOnlyList<MyCourseDto>>> GetMyCourses()
    {
        var userId = GetUserId();

        var enrollments = await _context.Enrollments
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Include(x => x.Course!)
                .ThenInclude(c => c.Category)
            .Include(x => x.Course!)
                .ThenInclude(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
            .ToListAsync();

        var result = enrollments.Select(enrollment =>
        {
            var course = enrollment.Course ?? throw new InvalidOperationException("Curso da matrícula não foi carregado");
            var lessons = course.Modules!
                .SelectMany(module => module!.Lessons)
                .Where(lesson => lesson.IsPublished)
                .ToList();

            var completedLessons = _context.LessonProgresses
                .AsNoTracking()
                .Count(progress => progress.UserId == userId && lessons.Select(l => l.Id).Contains(progress.LessonId) && progress.Completed);

            var watchedSeconds = _context.LessonProgresses
                .AsNoTracking()
                .Where(progress => progress.UserId == userId && lessons.Select(l => l.Id).Contains(progress.LessonId))
                .Sum(progress => progress.WatchedSeconds);

            var totalLessons = lessons.Count;
            var progressPercentage = totalLessons == 0 ? 0 : (int)Math.Round(completedLessons * 100.0 / totalLessons);

            return new MyCourseDto
            {
                CourseId = course.Id,
                Title = course.Title,
                Slug = course.Slug,
                ShortDescription = course.ShortDescription,
                Level = course.Level.ToString(),
                ThumbnailUrl = course.ThumbnailUrl,
                CategoryName = course.Category?.Name ?? string.Empty,
                CategorySlug = course.Category?.Slug ?? string.Empty,
                EnrolledAt = enrollment.EnrolledAt,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                WatchedSeconds = watchedSeconds,
                ProgressPercentage = progressPercentage
            };
        }).ToList();

        return Ok(result);
    }

    [HttpGet("courses/{courseId}/progress")]
    public async Task<ActionResult<CourseProgressDto>> GetCourseProgress(int courseId)
    {
        var userId = GetUserId();

        var enrollment = await _context.Enrollments
            .AsNoTracking()
            .Include(x => x.Course!)
                .ThenInclude(c => c.Modules)
                    .ThenInclude(m => m.Lessons)
            .FirstOrDefaultAsync(x => x.UserId == userId && x.CourseId == courseId);

        if (enrollment == null)
        {
            return NotFound("Matrícula não encontrada para este curso");
        }

        var course = enrollment.Course ?? throw new InvalidOperationException("Curso da matrícula não foi carregado");
        var lessons = course.Modules!
            .SelectMany(module => module!.Lessons)
            .Where(lesson => lesson.IsPublished)
            .ToList();

        var completedLessons = await _context.LessonProgresses
            .AsNoTracking()
            .CountAsync(progress => progress.UserId == userId && lessons.Select(l => l.Id).Contains(progress.LessonId) && progress.Completed);

        var watchedSeconds = await _context.LessonProgresses
            .AsNoTracking()
            .Where(progress => progress.UserId == userId && lessons.Select(l => l.Id).Contains(progress.LessonId))
            .SumAsync(progress => progress.WatchedSeconds);

        var totalLessons = lessons.Count;
        var progressPercentage = totalLessons == 0 ? 0 : (int)Math.Round(completedLessons * 100.0 / totalLessons);

        return Ok(new CourseProgressDto
        {
            CourseId = course.Id,
            CourseTitle = course.Title,
            CourseSlug = course.Slug,
            TotalLessons = totalLessons,
            CompletedLessons = completedLessons,
            WatchedSeconds = watchedSeconds,
            ProgressPercentage = progressPercentage
        });
    }

    [HttpPost("lessons/{lessonId}/progress")]
    public async Task<ActionResult<LessonProgressDto>> UpsertLessonProgress(int lessonId, [FromBody] LessonProgressUpsertDto request)
    {
        var userId = GetUserId();

        var lesson = await _context.Lessons
            .Include(x => x.CourseModule)
                .ThenInclude(module => module!.Course)
            .FirstOrDefaultAsync(x => x.Id == lessonId && x.IsPublished);

        if (lesson == null)
        {
            return NotFound("Aula não encontrada");
        }

        if (!lesson.IsPreview)
        {
            var courseModule = lesson.CourseModule ?? throw new InvalidOperationException("Módulo da aula não foi carregado");
            var isEnrolled = await _context.Enrollments.AnyAsync(x => x.UserId == userId && x.CourseId == courseModule.CourseId);
            if (!isEnrolled)
            {
                return Forbid();
            }
        }

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId);

        if (progress == null)
        {
            progress = new Models.LessonProgress
            {
                UserId = userId,
                LessonId = lessonId,
                WatchedSeconds = Math.Max(0, request.WatchedSeconds),
                Completed = request.Completed,
                LastAccessedAt = DateTime.UtcNow,
                CompletedAt = request.Completed ? DateTime.UtcNow : null
            };
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            progress.WatchedSeconds = Math.Max(progress.WatchedSeconds, request.WatchedSeconds);
            progress.Completed = request.Completed || progress.Completed;
            progress.LastAccessedAt = DateTime.UtcNow;
            if (progress.Completed && progress.CompletedAt == null)
            {
                progress.CompletedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();

        return Ok(new LessonProgressDto
        {
            Id = progress.Id,
            LessonId = progress.LessonId,
            WatchedSeconds = progress.WatchedSeconds,
            Completed = progress.Completed,
            LastAccessedAt = progress.LastAccessedAt,
            CompletedAt = progress.CompletedAt
        });
    }

    [HttpPost("lessons/{lessonId}/complete")]
    public async Task<ActionResult<LessonProgressDto>> CompleteLesson(int lessonId)
    {
        var userId = GetUserId();

        var lesson = await _context.Lessons
            .Include(x => x.CourseModule)
                .ThenInclude(module => module!.Course)
            .FirstOrDefaultAsync(x => x.Id == lessonId && x.IsPublished);

        if (lesson == null)
        {
            return NotFound("Aula não encontrada");
        }

        if (!lesson.IsPreview)
        {
            var courseModule = lesson.CourseModule ?? throw new InvalidOperationException("Módulo da aula não foi carregado");
            var isEnrolled = await _context.Enrollments.AnyAsync(x => x.UserId == userId && x.CourseId == courseModule.CourseId);
            if (!isEnrolled)
            {
                return Forbid();
            }
        }

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId);

        if (progress == null)
        {
            progress = new Models.LessonProgress
            {
                UserId = userId,
                LessonId = lessonId,
                WatchedSeconds = lesson.DurationInSeconds,
                Completed = true,
                LastAccessedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow
            };
            _context.LessonProgresses.Add(progress);
        }
        else
        {
            progress.Completed = true;
            progress.LastAccessedAt = DateTime.UtcNow;
            progress.CompletedAt ??= DateTime.UtcNow;
            progress.WatchedSeconds = Math.Max(progress.WatchedSeconds, lesson.DurationInSeconds);
        }

        await _context.SaveChangesAsync();

        return Ok(new LessonProgressDto
        {
            Id = progress.Id,
            LessonId = progress.LessonId,
            WatchedSeconds = progress.WatchedSeconds,
            Completed = progress.Completed,
            LastAccessedAt = progress.LastAccessedAt,
            CompletedAt = progress.CompletedAt
        });
    }

    private int GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(userIdClaim, out var userId) ? userId : 0;
    }
}
