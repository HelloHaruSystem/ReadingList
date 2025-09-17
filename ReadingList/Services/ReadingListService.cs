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

    public async Task<IEnumerable<UserBook>> GetBooksByStatusAsync(ReadingStatus status)
    {
        return await _userBookRepository.GetBooksByStatusAsync(status);
    }

    public async Task<IEnumerable<UserBook>> GetCurrentlyReadingBooksAsync()
    {
        return await _userBookRepository.GetBooksByStatusAsync(ReadingStatus.CurrentlyReading);
    }

    public async Task<IEnumerable<UserBook>> GetMyReadingListAsync()
    {
        return await _userBookRepository.GetUserReadingListAsync();
    }

    public async Task<IEnumerable<UserBook>> GetRecentlyCompletedBooksAsync(int count = 5)
    {
        return await _userBookRepository.GetRecentlyCompletedBooksAsync(count);
    }

    public async Task<IEnumerable<UserBook>> GetTopRatedBooksAsync(int count = 10)
    {
        return await _userBookRepository.GetTopRatedBooksAsync(count);
    }

    public async Task<bool> RateBookAsync(int userBookId, int? rating, string? notes = null)
    {
        return await _userBookRepository.RateBookAsync(userBookId, rating, notes);
    }

    public async Task<bool> RemoveFromListAsync(int userBookId)
    {
        return await _userBookRepository.RemoveFromListAsync(userBookId);
    }

    public async Task<bool> UpdateReadingStatusAsync(int userBookId, ReadingStatus newStatus)
    {
        return await _userBookRepository.UpdateReadingStatusAsync(userBookId, newStatus);
    }
}