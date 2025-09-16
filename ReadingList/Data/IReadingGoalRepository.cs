using ReadingList.Models;

namespace ReadingList.Data;

public interface IReadingGoalRepository
{
    Task<IEnumerable<ReadingGoal>> GetActiveGoalsAsync();
    Task<int> CreateGoalAsync(ReadingGoal goal);
    Task<bool> AddBookToGoalAsync(int goalId, string isbn);
    //Task<GoalProgress> GetGoalProgressAsync(int goalId); // maybe implement a GoalProgress
    Task<bool> MarkGoalCompleteAsync(int goalId);
}