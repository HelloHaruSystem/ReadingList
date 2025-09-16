using ReadingList.Models;
using ReadingList.Models.Enums;

namespace ReadingList.Services;

public interface IBookService
{
    Task<IEnumerable<Book>> GetAllBooksAsync();
    Task<Book?> GetBookWithDetailsAsync(string isbn);
    Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm);
    Task<IEnumerable<Book>> GetBooksBySubjectAsync(SubjectType subjectType);
    Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
}