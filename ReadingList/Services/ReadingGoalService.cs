using ReadingList.Data;
using ReadingList.Models;

namespace ReadingList.Services;

public class ReadingGoalService : IReadingGoalService
{

    private readonly IReadingGoalRepository _readingGoalRepository;

    public ReadingGoalService(IReadingGoalRepository readingGoalRepository)
    {
        _readingGoalRepository = readingGoalRepository;
    }

    public async Task<bool> AddBookToGoalAsync(int goalId, string isbn)
    {
        return await _readingGoalRepository.AddBookToGoalAsync(goalId, isbn);
    }

    public async Task<int> CreateGoalAsync(ReadingGoal goal)
    {
        return await _readingGoalRepository.CreateGoalAsync(goal);
    }

    public async Task<IEnumerable<ReadingGoal>> GetActiveGoalsAsync()
    {
        return await _readingGoalRepository.GetActiveGoalsAsync();
    }

    public Task<ReadingGoal> GetGoalProgressAsync(int goalId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ReadingGoal>> GetUpcomingDeadlinesAsync(int days = 7)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsGoalAchievedAsync(int goalId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> MarkGoalCompleteAsync(int goalId)
    {
        return await _readingGoalRepository.MarkGoalCompleteAsync(goalId);
    }
}