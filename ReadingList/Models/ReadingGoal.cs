namespace ReadingList.Models;

public class ReadingGoal
{
    public int Id { get; set; }
    public required string GoalName { get; set; }
    public string? Description { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime Deadline { get; set; }
    public int? TargetBooks { get; set; }
    public int? TargetPages { get; set; }
    public bool IsCompleted { get; set; }
    public List<Book> Books { get; set; } = new List<Book>();
}