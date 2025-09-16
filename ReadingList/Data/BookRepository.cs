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

    public async Task<Book?> GetWithDetailsAsync(string isbn)
    {
        Book? book = null;

        const string sql = @"
            SELECT DISTINCT
                b.isbn, b.title, b.publication_year, b.pages, b.description,
                a.id AS author_id, a.full_name AS author_name,
                s.id AS subject_id, s.subject_name,
                ub.reading_status, ub.personal_rating, ub.personal_notes
            FROM books AS b
            LEFT JOIN book_authors AS ba ON ba.book_isbn = b.isbn
            LEFT JOIN authors AS a ON a.id = ba.author_id
            LEFT JOIN book_subjects AS bs ON bs.book_isbn = b.isbn
            LEFT JOIN subjects AS s ON s.id = bs.subject_id
            LEFT JOIN user_books AS ub ON ub.book_isbn = b.isbn
            WHERE b.isbn = @isbn";

        using SqlConnection connection = new SqlConnection(_connectionString);
        using SqlCommand command = new SqlCommand(sql, connection);
        command.Parameters.AddWithValue("@isbn", isbn);

        try
        {
            await connection.OpenAsync();
            using SqlDataReader reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                if (book == null)
                {
                    book = new Book
                    {
                        ISBN = SqlDataReaderHelper.GetString(reader, "isbn"),
                        Title = SqlDataReaderHelper.GetString(reader, "title"),
                        PublicationYear = SqlDataReaderHelper.GetIntOrNull(reader, "publication_year"),
                        Pages = SqlDataReaderHelper.GetIntOrNull(reader, "pages"),
                        Description = SqlDataReaderHelper.GetStringOrNull(reader, "description"),
                        Authors = new List<Author>(),
                        Subjects = new List<Subject>()
                    };
                }

                // add author if author is not null
                if (!SqlDataReaderHelper.IsNull(reader, "author_id"))
                {
                    int authorId = SqlDataReaderHelper.GetInt(reader, "author_id");
                    if (!book.Authors.Any(a => a.Id == authorId))
                    {
                        book.Authors.Add(new Author
                        {
                            Id = authorId,
                            FullName = SqlDataReaderHelper.GetString(reader, "author_name")
                        });
                    }
                }

                // add subject if subject is not null
                if (!SqlDataReaderHelper.IsNull(reader, "subject_id"))
                {
                    int subjectId = SqlDataReaderHelper.GetInt(reader, "subject_id");
                    if (!book.Subjects.Any(s => s.Id == subjectId))
                    {
                        book.Subjects.Add(new Subject
                        {
                            Id = subjectId,
                            SubjectName = SqlDataReaderHelper.GetString(reader, "subject_name")
                        });
                    }
                }
            }

            return book;
        }
        catch (Exception ex)
        {
            Console.Write("Error fetching from DB:\n{0}\n", ex.Message);
            return null;
        }
    }

    public Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }
}