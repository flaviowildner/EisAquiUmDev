namespace Application.DTOs.Users;

public class LessonProgressUpsertDto
{
    public int WatchedSeconds { get; set; }
    public bool Completed { get; set; }
}
