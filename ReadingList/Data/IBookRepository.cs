using ReadingList.Models;

namespace ReadingList.Data;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
    Task<IEnumerable<Book>> GetBookBySubjectAsync(int subjectId);
    Task<Book> GetWithDetailsAsync(string isbn);
}