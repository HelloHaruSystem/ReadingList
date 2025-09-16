using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ReadingList.Models;
using ReadingList.Models.Enums;

namespace ReadingList.Data;

public class UserBookRepository : IUserBookRepository
{
    private readonly string _connectionString;

    public UserBookRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<int> AddToReadingListAsync(string isbn, ReadingStatus status = ReadingStatus.ToRead)
    {
        const string sql = @"
            INSERT INTO user_books (book_isbn, reading_status, date_started, updated_at)
            OUTPUT INSERTED.id
            VALUES
                (@isbn, @status, @dateStarted, @updatedAt)";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@isbn", isbn);
        command.Parameters.AddWithValue("@status", status.ToString().ToLower());
        command.Parameters.AddWithValue("@dateStarted", DateTime.Now);
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now);

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

    public Task<IEnumerable<UserBook>> GetBooksByStatusAsync(ReadingStatus status)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserBook>> GetUserReadingListAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> RateBookAsync(int userBookId, int rating, string notes = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateReadingStatusAsync(int userBookId, ReadingStatus status)
    {
        throw new NotImplementedException();
    }
}