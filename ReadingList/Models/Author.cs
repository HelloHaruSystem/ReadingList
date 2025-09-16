namespace ReadingList.Models;

public class Author
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public List<Book> books { get; set; } = new List<Book>();
}