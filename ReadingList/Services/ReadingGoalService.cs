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

    public async Task<GoalProgress> GetGoalProgressAsync(int goalId)
    {
        (ReadingGoal? goal, int booksAdded, int totalPages) = await _readingGoalRepository.GetGoalProgressAsync(goalId);

        if (goal == null)
        {
            return null;
        }

        // Calculate book progress
        bool isBookTargetMet = goal.TargetBooks == null || booksAdded >= goal.TargetBooks;
        double bookProgressPercentage = goal.TargetBooks != null ? (double)booksAdded / goal.TargetBooks.Value * 100 : 100;
    
        // Calculate page progress  
        bool isPageTargetMet = goal.TargetPages == null || totalPages >= goal.TargetPages;
        double pageProgressPercentage = goal.TargetPages != null ? (double)totalPages / goal.TargetPages.Value * 100 : 100;
    
        // Overall achievement
        bool isGoalAchieved = isBookTargetMet && isPageTargetMet;
    
        return new GoalProgress
        {
            Goal = goal,
            BooksAdded = booksAdded,
            TotalPages = totalPages,
            BookProgressPercentage = Math.Min(bookProgressPercentage, 100),
            PageProgressPercentage = Math.Min(pageProgressPercentage, 100),
            IsBookTargetMet = isBookTargetMet,
            IsPageTargetMet = isPageTargetMet,
            IsGoalAchieved = isGoalAchieved
        };
    }

    public async Task<IEnumerable<ReadingGoal>> GetUpcomingDeadlinesAsync(int days = 7)
    {
        IEnumerable<ReadingGoal> activeGoals = await _readingGoalRepository.GetActiveGoalsAsync();
        DateTime cutoffDate = DateTime.Now.AddDays(days);

        return activeGoals.Where(g => g.Deadline <= cutoffDate);
    }

    public async Task<bool> IsGoalAchievedAsync(int goalId)
    {
        GoalProgress progress = await GetGoalProgressAsync(goalId);
        return progress?.IsGoalAchieved ?? false;
    }

    public async Task<bool> MarkGoalCompleteAsync(int goalId)
    {
        return await _readingGoalRepository.MarkGoalCompleteAsync(goalId);
    }
}