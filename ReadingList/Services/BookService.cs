using ReadingList.Data;
using ReadingList.Models;

namespace ReadingList.Services;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<IEnumerable<Book>> GetAllBooksAsync()
    {
        return await _bookRepository.GetAllBooksAsync();
    }

    public Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> GetBooksBySubjectAsync(int subjectId)
    {
        throw new NotImplementedException();
    }

    public Task<Book> GetBookWithDetailsAsync(string isbn)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> GetRecentlyAddedBooksAsync(int count = 10)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        throw new NotImplementedException();
    }
}