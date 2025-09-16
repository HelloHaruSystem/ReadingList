namespace ReadingList.Models;

public class Subject
{
    public int Id { get; set; }
    public string SubjectName { get; set; }
    public List<Book> Books { get; set; } = new List<Book>();
}