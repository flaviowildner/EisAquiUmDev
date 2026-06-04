namespace Application.DTOs.Courses;

public class LessonDetailsDto
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseSlug { get; set; } = string.Empty;
    public int ModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int DurationInSeconds { get; set; }
    public int Order { get; set; }
    public bool IsPreview { get; set; }
}
