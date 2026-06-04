namespace Application.DTOs.Users;

public class LessonProgressDto
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public int WatchedSeconds { get; set; }
    public bool Completed { get; set; }
    public DateTime LastAccessedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
