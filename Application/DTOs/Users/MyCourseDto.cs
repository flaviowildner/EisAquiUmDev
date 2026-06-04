namespace Application.DTOs.Users;

public class MyCourseDto
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ShortDescription { get; set; } = string.Empty;
    public string Level { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public int WatchedSeconds { get; set; }
    public int ProgressPercentage { get; set; }
}
