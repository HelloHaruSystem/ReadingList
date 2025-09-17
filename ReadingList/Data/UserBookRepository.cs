using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ReadingList.Models;
using ReadingList.Models.Enums;
using ReadingList.Utils;

namespace ReadingList.Data;

public class UserBookRepository : IUserBookRepository
{
    private readonly string _connectionString;

    public UserBookRepository(IConfiguration configuration)
    {
        _connectionString = ConnectionStringHelper.GetRequiredConnectionString(configuration, "DefaultConnection");
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

    public async Task<IEnumerable<UserBook>> GetBooksByStatusAsync(ReadingStatus status)
    {
        List<UserBook> userBooks = new List<UserBook>();

        const string sql = @"
            SELECT 
                ub.*,
                b.title, b.publication_year, b.pages, b.description
            FROM user_books AS ub
            INNER JOIN books AS b
            ON b.isbn = ub.book_isbn
            WHERE ub.reading_status = @status
            ORDER BY ub.updated_at DESC";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@status", status.ToString().ToLower());

        try
        {
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                userBooks.Add(new UserBook
                {
                    Id = SqlDataReaderHelper.GetInt(reader, "id"),
                    BookISBN = SqlDataReaderHelper.GetString(reader, "book_isbn"),
                    Status = Enum.Parse<ReadingStatus>(SqlDataReaderHelper.GetString(reader, "reading_status"), true),
                    PersonalRating = SqlDataReaderHelper.GetIntOrNull(reader, "personal_rating"),
                    PersonalNotes = SqlDataReaderHelper.GetStringOrNull(reader, "personal_notes"),
                    DateStarted = SqlDataReaderHelper.GetDateTime(reader, "date_started"),
                    UpdatedAt = SqlDataReaderHelper.GetDateTime(reader, "updated_at"),
                    Book = new Book
                    {
                        ISBN = SqlDataReaderHelper.GetString(reader, "book_isbn"),
                        Title = SqlDataReaderHelper.GetString(reader, "title"),
                        PublicationYear = SqlDataReaderHelper.GetIntOrNull(reader, "publication_year"),
                        Pages = SqlDataReaderHelper.GetIntOrNull(reader, "pages"),
                        Description = SqlDataReaderHelper.GetStringOrNull(reader, "description")
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
            return new List<UserBook>();
        }

        return userBooks;
    }

    public async Task<IEnumerable<UserBook>> GetRecentlyCompletedBooksAsync(int count)
    {
        List<UserBook> recentlyCompletedBooks = new List<UserBook>();

        const string sql = @"
            SELECT TOP (@count)
                ub.*,
                b.title, b.publication_year, b.pages, b.description
            FROM user_books AS ub
            INNER JOIN books AS b
            ON b.isbn = ub.book_isbn
            WHERE ub.reading_status = @status
            ORDER BY ub.updated_at DESC";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@count", count);
        command.Parameters.AddWithValue("@status", ReadingStatus.Completed.ToString().ToLower());
        
        try
        {
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                recentlyCompletedBooks.Add(new UserBook
                {
                    Id = SqlDataReaderHelper.GetInt(reader, "id"),
                    BookISBN = SqlDataReaderHelper.GetString(reader, "book_isbn"),
                    Status = Enum.Parse<ReadingStatus>(SqlDataReaderHelper.GetString(reader, "reading_status"), true),
                    PersonalRating = SqlDataReaderHelper.GetIntOrNull(reader, "personal_rating"),
                    PersonalNotes = SqlDataReaderHelper.GetStringOrNull(reader, "personal_notes"),
                    DateStarted = SqlDataReaderHelper.GetDateTime(reader, "date_started"),
                    UpdatedAt = SqlDataReaderHelper.GetDateTime(reader, "updated_at"),
                    Book = new Book
                    {
                        ISBN = SqlDataReaderHelper.GetString(reader, "book_isbn"),
                        Title = SqlDataReaderHelper.GetString(reader, "title"),
                        PublicationYear = SqlDataReaderHelper.GetIntOrNull(reader, "publication_year"),
                        Pages = SqlDataReaderHelper.GetIntOrNull(reader, "pages"),
                        Description = SqlDataReaderHelper.GetStringOrNull(reader, "description")
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
            return new List<UserBook>();
        }

        return recentlyCompletedBooks;
    }

    public async Task<IEnumerable<UserBook>> GetTopRatedBooksAsync(int count)
    {
        List<UserBook> topRatedBooks = new List<UserBook>();

        const string sql = @"
            Select TOP (@count)
                ub.*,
                b.title, b.publication_year, b.pages, b.description
            FROM user_books AS ub
            INNER JOIN books AS b
            ON b.isbn = ub.book_isbn
            WHERE ub.personal_rating IS NOT NULL
            ORDER BY 
                ub.personal_rating DESC,
                ub.updated_at DESC";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@count", count);
        
        try
        {
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                topRatedBooks.Add(new UserBook
                {
                    Id = SqlDataReaderHelper.GetInt(reader, "id"),
                    BookISBN = SqlDataReaderHelper.GetString(reader, "book_isbn"),
                    Status = Enum.Parse<ReadingStatus>(SqlDataReaderHelper.GetString(reader, "reading_status"), true),
                    PersonalRating = SqlDataReaderHelper.GetIntOrNull(reader, "personal_rating"),
                    PersonalNotes = SqlDataReaderHelper.GetStringOrNull(reader, "personal_notes"),
                    DateStarted = SqlDataReaderHelper.GetDateTime(reader, "date_started"),
                    UpdatedAt = SqlDataReaderHelper.GetDateTime(reader, "updated_at"),
                    Book = new Book
                    {
                        ISBN = SqlDataReaderHelper.GetString(reader, "book_isbn"),
                        Title = SqlDataReaderHelper.GetString(reader, "title"),
                        PublicationYear = SqlDataReaderHelper.GetIntOrNull(reader, "publication_year"),
                        Pages = SqlDataReaderHelper.GetIntOrNull(reader, "pages"),
                        Description = SqlDataReaderHelper.GetStringOrNull(reader, "description")
                    }
                });
            }
        }   
        catch (Exception ex)
        {
            Console.Write("Error fetching top rated books from DB:\n{0}\n", ex.Message);
            return new List<UserBook>();
        }

        return topRatedBooks;
    }

    public async Task<IEnumerable<UserBook>> GetUserReadingListAsync()
    {
        List<UserBook> userBooks = new List<UserBook>();

        const string sql = @"
            SELECT 
                ub.*,
                b.title, b.publication_year, b.pages, b.description
            FROM user_books AS ub
            INNER JOIN books AS b
            ON b.isbn = ub.book_isbn
            ORDER BY ub.updated_at DESC";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        try
        {
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                userBooks.Add(new UserBook
                {
                    Id = SqlDataReaderHelper.GetInt(reader, "id"),
                    BookISBN = SqlDataReaderHelper.GetString(reader, "book_isbn"),
                    Status = Enum.Parse<ReadingStatus>(SqlDataReaderHelper.GetString(reader, "reading_status"), true),
                    PersonalRating = SqlDataReaderHelper.GetIntOrNull(reader, "personal_rating"),
                    PersonalNotes = SqlDataReaderHelper.GetStringOrNull(reader, "personal_notes"),
                    DateStarted = SqlDataReaderHelper.GetDateTime(reader, "date_started"),
                    UpdatedAt = SqlDataReaderHelper.GetDateTime(reader, "updated_at"),
                    Book = new Book
                    {
                        ISBN = SqlDataReaderHelper.GetString(reader, "book_isbn"),
                        Title = SqlDataReaderHelper.GetString(reader, "title"),
                        PublicationYear = SqlDataReaderHelper.GetIntOrNull(reader, "publication_year"),
                        Pages = SqlDataReaderHelper.GetIntOrNull(reader, "pages"),
                        Description = SqlDataReaderHelper.GetStringOrNull(reader, "description")
                    }
                });
            }
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
            return new List<UserBook>();
        }

        return userBooks;
    }

    public async Task<bool> RateBookAsync(int userBookId, int rating, string? notes = null)
    {
        string sql = @"
            UPDATE user_books
            SET personal_rating = @rating, updated_at = @updatedAt";

        if (notes != null)
        {
            sql += ", personal_notes = @notes";
        }
        sql += " WHERE id = @userBookId";


        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@rating", rating);
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now);
        command.Parameters.AddWithValue("@userBookId", userBookId);

        if (notes != null)
        {
            command.Parameters.AddWithValue("@notes", notes);
        }

        try
        {
            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.Write("Error rating book:\n{0}\n", ex.Message);
            return false;
        }
    }

    public async Task<bool> RemoveFromListAsync(int userBookId)
    {
        const string sql = @"
            DELETE FROM user_books
            WHERE id = @userBookId";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@userBookId", userBookId);

        try
        {
            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.Write("Error deleting book from list:.\n{0}\n", ex.Message);
            return false;
        }
    }

    public async Task<bool> UpdateReadingStatusAsync(int userBookId, ReadingStatus status)
    {
        const string sql = @"
        UPDATE user_books
        SET reading_status = @status, updated_at = @updatedAt
        WHERE id = @userBookId";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@status", status.ToString().ToLower());
        command.Parameters.AddWithValue("@updatedAt", DateTime.Now);
        command.Parameters.AddWithValue("@userBookId", userBookId);

        try
        {
            await connection.OpenAsync();
            int rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            Console.Write("Error updating status book:\n{0}\n", ex.Message);
            return false;
        }
    }
}