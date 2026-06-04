namespace Application.DTOs.Users;

public class CourseProgressDto
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseSlug { get; set; } = string.Empty;
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int WatchedSeconds { get; set; }
    public int ProgressPercentage { get; set; }
}
