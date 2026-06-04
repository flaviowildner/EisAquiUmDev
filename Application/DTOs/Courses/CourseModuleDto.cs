namespace Application.DTOs.Courses;

public class CourseModuleDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Order { get; set; }
    public int TotalDurationInSeconds { get; set; }
    public IReadOnlyList<LessonSummaryDto> Lessons { get; set; } = [];
}
