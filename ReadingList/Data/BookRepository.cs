using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using ReadingList.Models;
using ReadingList.Utils;

namespace ReadingList.Data;

public class BookRepository : IBookRepository
{
    private readonly string _connectionString;

    public BookRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        List<Book> books = new List<Book>();

        const string sql = @"
            SELECT *
            FROM books
            ORDER BY title";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);

        await connection.OpenAsync();
        using SqlDataReader reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            books.Add(new Book
            {
                ISBN = SqlDataReaderHelper.GetString(reader, "isbn"),
                Title = SqlDataReaderHelper.GetString(reader, "title"),
                PublicationYear = SqlDataReaderHelper.GetIntOrNull(reader, "publication_year"),
                Pages = SqlDataReaderHelper.GetIntOrNull(reader, "pages"),
                Description = SqlDataReaderHelper.GetStringOrNull(reader, "description")
            });
        }

        return books;
    }

    public Task<IEnumerable<Book>> GetBookBySubjectAsync(int subjectId)
    {
        throw new NotImplementedException();
    }

    public Task<Book> GetWithDetailsAsync(string isbn)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }
}