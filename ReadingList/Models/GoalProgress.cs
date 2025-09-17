namespace ReadingList.Models;

public class GoalProgress
{
    public ReadingGoal? Goal { get; set; }
    public int BooksAdded { get; set; }
    public int TotalPages { get; set; }
    public double BookProgressPercentage { get; set; }
    public double PageProgressPercentage { get; set; }
    public bool IsBookTargetMet { get; set; }
    public bool IsPageTargetMet { get; set; }
    public bool IsGoalAchieved { get; set; }
}