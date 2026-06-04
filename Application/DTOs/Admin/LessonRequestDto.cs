namespace Application.DTOs.Admin;

public class LessonRequestDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public int DurationInSeconds { get; set; }
    public int Order { get; set; }
    public bool IsPreview { get; set; }
    public bool IsPublished { get; set; }
}
