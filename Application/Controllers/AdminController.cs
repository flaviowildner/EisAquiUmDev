using Application.Data;
using Application.DTOs.Admin;
using Application.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("courses")]
    public async Task<ActionResult> CreateCourse([FromBody] CourseRequestDto request)
    {
        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return BadRequest("Categoria não encontrada");
        }

        var existingSlug = await _context.Courses.AnyAsync(x => x.Slug == request.Slug);
        if (existingSlug)
        {
            return BadRequest("Slug de curso já existe");
        }

        var course = new Course
        {
            CategoryId = request.CategoryId,
            Title = request.Title,
            Slug = request.Slug,
            ShortDescription = request.ShortDescription,
            Description = request.Description,
            Level = request.Level,
            ThumbnailUrl = request.ThumbnailUrl,
            IsPublished = request.IsPublished,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return CreatedAtAction(null, new { id = course.Id }, course);
    }

    [HttpPut("courses/{id}")]
    public async Task<ActionResult> UpdateCourse(int id, [FromBody] CourseRequestDto request)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound("Curso não encontrado");
        }

        var category = await _context.Categories.FindAsync(request.CategoryId);
        if (category == null)
        {
            return BadRequest("Categoria não encontrada");
        }

        if (course.Slug != request.Slug)
        {
            var slugExists = await _context.Courses.AnyAsync(x => x.Slug == request.Slug && x.Id != id);
            if (slugExists)
            {
                return BadRequest("Slug de curso já existe");
            }
        }

        course.CategoryId = request.CategoryId;
        course.Title = request.Title;
        course.Slug = request.Slug;
        course.ShortDescription = request.ShortDescription;
        course.Description = request.Description;
        course.Level = request.Level;
        course.ThumbnailUrl = request.ThumbnailUrl;
        course.IsPublished = request.IsPublished;
        course.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("courses/{id}")]
    public async Task<ActionResult> DeleteCourse(int id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
        {
            return NotFound("Curso não encontrado");
        }

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("courses/{courseId}/modules")]
    public async Task<ActionResult> CreateModule(int courseId, [FromBody] ModuleRequestDto request)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
        {
            return NotFound("Curso não encontrado");
        }

        var module = new CourseModule
        {
            CourseId = courseId,
            Title = request.Title,
            Description = request.Description,
            Order = request.Order
        };

        _context.CourseModules.Add(module);
        await _context.SaveChangesAsync();

        return CreatedAtAction(null, new { id = module.Id }, module);
    }

    [HttpPut("modules/{id}")]
    public async Task<ActionResult> UpdateModule(int id, [FromBody] ModuleRequestDto request)
    {
        var module = await _context.CourseModules.FindAsync(id);
        if (module == null)
        {
            return NotFound("Módulo não encontrado");
        }

        module.Title = request.Title;
        module.Description = request.Description;
        module.Order = request.Order;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("modules/{id}")]
    public async Task<ActionResult> DeleteModule(int id)
    {
        var module = await _context.CourseModules.FindAsync(id);
        if (module == null)
        {
            return NotFound("Módulo não encontrado");
        }

        _context.CourseModules.Remove(module);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("modules/{moduleId}/lessons")]
    public async Task<ActionResult> CreateLesson(int moduleId, [FromBody] LessonRequestDto request)
    {
        var module = await _context.CourseModules.FindAsync(moduleId);
        if (module == null)
        {
            return NotFound("Módulo não encontrado");
        }

        var lesson = new Lesson
        {
            CourseModuleId = moduleId,
            Title = request.Title,
            Slug = request.Slug,
            Summary = request.Summary,
            Content = request.Content,
            VideoUrl = request.VideoUrl,
            DurationInSeconds = request.DurationInSeconds,
            Order = request.Order,
            IsPreview = request.IsPreview,
            IsPublished = request.IsPublished
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        return CreatedAtAction(null, new { id = lesson.Id }, lesson);
    }

    [HttpPut("lessons/{id}")]
    public async Task<ActionResult> UpdateLesson(int id, [FromBody] LessonRequestDto request)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null)
        {
            return NotFound("Aula não encontrada");
        }

        lesson.Title = request.Title;
        lesson.Slug = request.Slug;
        lesson.Summary = request.Summary;
        lesson.Content = request.Content;
        lesson.VideoUrl = request.VideoUrl;
        lesson.DurationInSeconds = request.DurationInSeconds;
        lesson.Order = request.Order;
        lesson.IsPreview = request.IsPreview;
        lesson.IsPublished = request.IsPublished;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("lessons/{id}")]
    public async Task<ActionResult> DeleteLesson(int id)
    {
        var lesson = await _context.Lessons.FindAsync(id);
        if (lesson == null)
        {
            return NotFound("Aula não encontrada");
        }

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
