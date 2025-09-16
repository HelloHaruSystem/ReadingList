using ReadingList.Models;

namespace ReadingList.Services;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book> GetBookWithDetailsAsync(string isbn);
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
    Task<IEnumerable<Book>> GetBooksBySubjectAsync(int subjectId);
    Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
    Task<IEnumerable<Book>> GetRecentlyAddedBooksAsync(int count = 10);
}