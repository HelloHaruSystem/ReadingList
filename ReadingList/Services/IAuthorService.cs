using ReadingList.Models;

namespace ReadingList.Services;

public interface IAuthorService
{
    Task<IEnumerable<Author>> GetAllAuthorsAsync();
    Task<Author> GetAuthorByIdAsync(int id);
    Task<IEnumerable<Author>> SearchAuthorsByNameAsync(string name);
    Task<IEnumerable<Book>> GetBooksByAuthorAsync(int authorId);
    Task<IEnumerable<Author>> GetMostReadAuthorsAsync(int count = 10);
}