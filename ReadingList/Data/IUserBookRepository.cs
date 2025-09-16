using ReadingList.Models;
using ReadingList.Models.Enums;

namespace ReadingList.Data;

public interface IUserBookRepository
{
    Task<IEnumerable<UserBook>> GetUserReadingListAsync();
    Task<IEnumerable<UserBook>> GetBooksByStatusAsync(ReadingStatus status);
    Task<int> AddToReadingListAsync(string isbn, ReadingStatus status = ReadingStatus.ToRead);
    Task<bool> UpdateReadingStatusAsync(int userBookId, ReadingStatus status);
    Task<bool> RateBookAsync(int userBookId, int rating, string? notes = null);
    Task<bool> RemoveFromListAsync(int userBookId);
    Task<IEnumerable<UserBook>> GetRecentlyCompletedBooksAsync(int count);
    Task<IEnumerable<UserBook>> GetTopRatedBooksAsync(int count);

}