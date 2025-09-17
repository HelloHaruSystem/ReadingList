using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ReadingList.Models;
using ReadingList.Utils;

namespace ReadingList.Data;

public class ReadingGoalRepository : IReadingGoalRepository
{
    private readonly string _connectionString;

    public ReadingGoalRepository(IConfiguration configuration)
    {
        _connectionString = ConnectionStringHelper.GetRequiredConnectionString(configuration, "DefaultConnection");
    }

    public async Task<bool> AddBookToGoalAsync(int goalId, string isbn)
    {
        const string sql = @"
            INSERT INTO goal_books (goal_id, book_isbn)
            VALUES
                (@goalId, @isbn)";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@goalId", goalId);
        command.Parameters.AddWithValue("@isbn", isbn);

        try
        {
            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.Write("Error adding book to goal:\n{0}\n", ex.Message);
            return false;
        }
    }

    public async Task<int> CreateGoalAsync(ReadingGoal goal)
    {
        const string sql = @"
            INSERT INTO reading_goals (goal_name, description, start_date, deadline, target_books, target_pages)
            OUTPUT INSERTED.id
            VALUES
                (@goalName, @description, @startDate, @deadline, @targetBooks, @targetPages)";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@goalName", goal.GoalName);
        SqlCommandHelper.AddParameterWithNullCheck(command, "@description", goal.Description);
        command.Parameters.AddWithValue("@startDate", goal.StartDate);
        command.Parameters.AddWithValue("@deadline", goal.Deadline);
        SqlCommandHelper.AddParameterWithNullCheck(command, "@targetBooks", goal.TargetBooks);
        SqlCommandHelper.AddParameterWithNullCheck(command, "@targetPages", goal.TargetPages);

        try
        {
            await connection.OpenAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
            return -1;
        }
    }

    public async Task<IEnumerable<ReadingGoal>> GetActiveGoalsAsync()
    {
        List<ReadingGoal> activeGoals = new List<ReadingGoal>();

        const string sql = @"
            SELECT *
            FROM reading_goals
            WHERE is_completed = 0
            ORDER BY deadline ASC";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        try
        {
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                activeGoals.Add(new ReadingGoal
                {
                    Id = SqlDataReaderHelper.GetInt(reader, "id"),
                    GoalName = SqlDataReaderHelper.GetString(reader, "goal_name"),
                    Description = SqlDataReaderHelper.GetStringOrNull(reader, "description"),
                    StartDate = SqlDataReaderHelper.GetDateTime(reader, "start_date"),
                    Deadline = SqlDataReaderHelper.GetDateTime(reader, "deadline"),
                    TargetBooks = SqlDataReaderHelper.GetIntOrNull(reader, "target_books"),
                    TargetPages = SqlDataReaderHelper.GetIntOrNull(reader, "target_pages"),
                    IsCompleted = SqlDataReaderHelper.GetBool(reader, "is_completed")
                });
            }
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
            return new List<ReadingGoal>();
        }

        return activeGoals;
    }

    public async Task<bool> MarkGoalCompleteAsync(int goalId)
    {
        const string sql = @"
            UPDATE reading_goals
            SET is_completed = 1
            WHERE id = @goalId";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@goalId", goalId);

        try
        {
            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
            return false;
        }
    }

    public async Task<(ReadingGoal? goal, int booksAdded, int totalPages)> GetGoalProgressAsync(int goalId)
    {
        ReadingGoal? goal = null;

        const string sql = @"
            SELECT 
                rg.*,
                COUNT(gb.book_isbn) AS books_added,
                SUM(
                    CASE WHEN
                        b.pages IS NOT NULL THEN b.pages ELSE 0
                    END
                ) AS total_pages
            FROM reading_goals AS rg
            LEFT JOIN goal_books AS gb
            ON gb.goal_id = rg.id
            LEFT JOIN books AS b
            ON b.isbn = gb.book_isbn
            WHERE rg.id = @goalId
            GROUP BY
                rg.id, rg.goal_name,
                rg.description, rg.start_date,
                rg.deadline, rg.target_books,
                rg.target_pages, rg.is_completed";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@goalId", goalId);

        try
        {
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                goal = new ReadingGoal
                {
                    Id = SqlDataReaderHelper.GetInt(reader, "id"),
                    GoalName = SqlDataReaderHelper.GetString(reader, "goal_name"),
                    Description = SqlDataReaderHelper.GetStringOrNull(reader, "description"),
                    StartDate = SqlDataReaderHelper.GetDateTime(reader, "start_date"),
                    Deadline = SqlDataReaderHelper.GetDateTime(reader, "deadline"),
                    TargetBooks = SqlDataReaderHelper.GetIntOrNull(reader, "target_books"),
                    TargetPages = SqlDataReaderHelper.GetIntOrNull(reader, "target_pages"),
                    IsCompleted = SqlDataReaderHelper.GetBool(reader, "is_completed"),
                };

                int booksAdded = SqlDataReaderHelper.GetIntOrNull(reader, "books_added") ?? 0;
                int totalPages = SqlDataReaderHelper.GetIntOrNull(reader, "total_pages") ?? 0;
            
                return (goal, booksAdded, totalPages);
            }
            
            return (null, 0, 0);
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching goal progress from DB:\n{0}\n", ex.Message);
            return (null, 0, 0);
        }
    }
}