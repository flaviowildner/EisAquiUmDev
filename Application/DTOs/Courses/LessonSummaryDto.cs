namespace Application.DTOs.Courses;

public class LessonSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int DurationInSeconds { get; set; }
    public int Order { get; set; }
    public bool IsPreview { get; set; }
}
