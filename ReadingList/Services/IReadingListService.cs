using ReadingList.Models;
using ReadingList.Models.Enums;

namespace ReadingList.Services;

public interface IReadingListService
{
    Task<IEnumerable<UserBook>> GetMyReadingListAsync();
    Task<IEnumerable<UserBook>> GetBooksByStatusAsync(ReadingStatus status);
    Task<int> AddBookToListAsync(string isbn, ReadingStatus status = ReadingStatus.ToRead);
    Task<bool> UpdateReadingStatusAsync(int userBookId, ReadingStatus newStatus);
    Task<bool> RateBookAsync(int userBookId, int rating, string? notes = null);
    Task<bool> RemoveFromListAsync(int userBookId);
    Task<IEnumerable<UserBook>> GetRecentlyCompletedBooksAsync(int count = 5);
    Task<IEnumerable<UserBook>> GetCurrentlyReadingBooksAsync();
    Task<IEnumerable<UserBook>> GetTopRatedBooksAsync(int count = 10);
}