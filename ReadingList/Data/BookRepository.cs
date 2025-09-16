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

        try
        {
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
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
        }

        return books;
    }

    public async Task<IEnumerable<Book>> GetBookBySubjectAsync(int subjectId = 1)
    {
        List<Book> booksOfSelectedSubject = new List<Book>();

        const string sql = @"
            SELECT b.*
            FROM books AS b
            INNER JOIN book_subjects AS bs
            ON bs.book_isbn = b.isbn
            WHERE bs.subject_id = @subjectId
            ORDER BY b.title";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@subjectId", subjectId);

        try
        {
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                booksOfSelectedSubject.Add(new Book
                {
                    ISBN = SqlDataReaderHelper.GetString(reader, "isbn"),
                    Title = SqlDataReaderHelper.GetString(reader, "title"),
                    PublicationYear = SqlDataReaderHelper.GetIntOrNull(reader, "publication_year"),
                    Pages = SqlDataReaderHelper.GetIntOrNull(reader, "pages"),
                    Description = SqlDataReaderHelper.GetStringOrNull(reader, "description")
                });
            }
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
        }
        return booksOfSelectedSubject;
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