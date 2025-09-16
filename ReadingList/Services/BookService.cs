using ReadingList.Data;
using ReadingList.Models;
using ReadingList.Models.Enums;

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

    public async Task<IEnumerable<Book>> GetBooksBySubjectAsync(SubjectType subjectType)
    {
        return await _bookRepository.GetBookBySubjectAsync((int)subjectType);
    }

    public async Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId)
    {
        return await _bookRepository.GetBooksByAuthorAsync(authorId);
    }

    public async Task<Book?> GetBookWithDetailsAsync(string isbn)
    {
        return await _bookRepository.GetWithDetailsAsync(isbn);
    }

    public Task<IEnumerable<Book>> GetRecentlyAddedBooksAsync(int count = 10)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Book>> SearchBooksAsync(string searchTerm)
    {
        return await _bookRepository.SearchBooksAsync(searchTerm);
    }
}