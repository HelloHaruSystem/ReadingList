using ReadingList.Models;

namespace ReadingList.Data;

public interface IReadingGoalRepository
{
    Task<IEnumerable<ReadingGoal>> GetActiveGoalsAsync();
    Task<int> CreateGoalAsync(ReadingGoal goal);
    Task<bool> AddBookToGoalAsync(int goalId, string isbn);
    Task<bool> MarkGoalCompleteAsync(int goalId);
    Task<(ReadingGoal? goal, int booksAdded, int totalPages)> GetGoalProgressAsync(int goalId);
}