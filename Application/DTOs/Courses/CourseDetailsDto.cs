namespace Application.DTOs.Courses;

public class CourseDetailsDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public int TotalDurationInSeconds { get; set; }
    public int ModuleCount { get; set; }
    public int LessonCount { get; set; }
    public IReadOnlyList<CourseModuleDto> Modules { get; set; } = [];
}
