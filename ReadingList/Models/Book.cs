namespace ReadingList.Models;

public class Book
{
    public string ISBN { get; set; }
    public string Title { get; set; }
    public int? PublicationYear { get; set; }
    public int? Pages { get; set; }
    public string Description { get; set; }

    // nav
    public List<Author> Authors { get; set; } = new List<Author>();
    public List<Subject> Subjects { get; set; } = new List<Subject>();
}