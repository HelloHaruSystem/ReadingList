using Microsoft.Extensions.Configuration;
using ReadingList.Models;

namespace ReadingList.Data;

public class ReadingGoalRepository : IReadingGoalRepository
{
    private readonly string _connectionString;

    public ReadingGoalRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public Task<bool> AddBookToGoalAsync(int goalId, string isbn)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateGoalAsync(ReadingGoal goal)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<ReadingGoal>> GetActiveGoalsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> MarkGoalCompleteAsync(int goalId)
    {
        throw new NotImplementedException();
    }
}