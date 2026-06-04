namespace Application.Models;

public class LessonProgress
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int LessonId { get; set; }
    public int WatchedSeconds { get; set; }
    public bool Completed { get; set; }
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    public User? User { get; set; }
    public Lesson? Lesson { get; set; }
}
