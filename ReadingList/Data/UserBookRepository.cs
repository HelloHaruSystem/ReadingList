using ReadingList.Models;
using ReadingList.Models.Enums;

namespace ReadingList.Data;

public class UserBookRepository : IUserBookRepository
{
    public Task<int> AddToReadingListAsync(string isbn, ReadingStatus status = ReadingStatus.ToRead)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserBook>> GetBooksByStatusAsync(ReadingStatus status)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserBook>> GetUserReadingListAsync()
    {
        throw new NotImplementedException();
    }

    public Task<bool> RateBookAsync(int userBookId, int rating, string notes = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateReadingStatusAsync(int userBookId, ReadingStatus status)
    {
        throw new NotImplementedException();
    }
}