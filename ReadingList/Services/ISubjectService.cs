using ReadingList.Models;

namespace ReadingList.Services;

public interface ISubjectService
{
    Task<IEnumerable<Subject>> GetAllSubjectsAsync();
    Task<IEnumerable<Book>> GetBooksBySubjectAsync(int subjectId);
    Task<Dictionary<Subject, int>> GetSubjectBookCountsAsync();
    Task<IEnumerable<Subject>> GetMyTopSubjectsAsync(int count = 5);
    Task<Subject> GetSubjectByIdAsync(int id);
}