namespace Application.DTOs.Admin;

using Application.Models;

public class CourseRequestDto
{
    public int CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public bool IsPublished { get; set; }
}
