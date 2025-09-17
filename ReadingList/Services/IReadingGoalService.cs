using ReadingList.Models;

namespace ReadingList.Services;

public interface IReadingGoalService
{
    Task<IEnumerable<ReadingGoal>> GetActiveGoalsAsync();
    Task<int> CreateGoalAsync(ReadingGoal goal);
    Task<bool> AddBookToGoalAsync(int goalId, string isbn);
    Task<bool> MarkGoalCompleteAsync(int goalId);
    Task<GoalProgress?> GetGoalProgressAsync(int goalId);
    Task<bool> IsGoalAchievedAsync(int goalId);
    Task<IEnumerable<ReadingGoal>> GetUpcomingDeadlinesAsync(int days = 7);
}