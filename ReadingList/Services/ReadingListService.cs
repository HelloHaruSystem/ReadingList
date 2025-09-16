using ReadingList.Data;
using ReadingList.Models;
using ReadingList.Models.Enums;

namespace ReadingList.Services;

public class ReadingListService : IReadingListService
{

    private readonly IUserBookRepository _userBookRepository;

    public ReadingListService(IUserBookRepository userBookRepository)
    {
        _userBookRepository = userBookRepository;
    }

    public async Task<int> AddBookToListAsync(string isbn, ReadingStatus status = ReadingStatus.ToRead)
    {
        return await _userBookRepository.AddToReadingListAsync(isbn, status);
    }

    public Task<IEnumerable<UserBook>> GetBooksByStatusAsync(ReadingStatus status)
    {
        throw new NotImplementedException();
    }

    public Task<UserBook> GetCurrentlyReadingBookAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserBook>> GetMyReadingListAsync()
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserBook>> GetRecentlyCompletedBooksAsync(int count = 5)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserBook>> GetTopRatedBooksAsync(int count = 10)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RateBookAsync(int userBookId, int rating, string notes = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> RemoveFromListAsync(int userBookId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateReadingStatusAsync(int userBookId, ReadingStatus newStatus)
    {
        throw new NotImplementedException();
    }
}