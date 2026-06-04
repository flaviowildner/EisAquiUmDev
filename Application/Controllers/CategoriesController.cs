using Application.Data;
using Application.DTOs.Categories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Application.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetCategories()
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                PublishedCourseCount = x.Courses.Count(course => course.IsPublished)
            })
            .ToListAsync();

        return Ok(categories);
    }
}
