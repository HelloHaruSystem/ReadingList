using Microsoft.Extensions.Configuration;
using ReadingList.Data;
using ReadingList.Models;
using ReadingList.Models.Enums;
using ReadingList.Services;
using ReadingList.Views;
using Terminal.Gui;

Console.WriteLine("Hello, World!");


// test crud stuff
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

if (!(config.GetConnectionString("DefaultConnection") == null))
{
    IBookRepository bookRepository = new BookRepository(config);
    IBookService bookService = new BookService(bookRepository);

    List<Book> books = (await bookService.GetBooksBySubjectAsync(SubjectType.Algorithms)).ToList();

    foreach (Book b in books)
    {
        Console.Write("{0} ({1})\n", b.Title, b.PublicationYear);
    }

    Book? details = await bookService.GetBookWithDetailsAsync("9780131103627");
    if (details != null)
    {
        Console.WriteLine("=== Book Details ===");
        Console.WriteLine("ISBN: {0}", details.ISBN);
        Console.WriteLine("Title: {0}", details.Title);
        Console.WriteLine("Publication Year: {0}", details.PublicationYear?.ToString() ?? "N/A");
        Console.WriteLine("Pages: {0}", details.Pages?.ToString() ?? "N/A");
        Console.WriteLine("Description: {0}", details.Description ?? "No description available");
    
        Console.WriteLine("\nAuthors:");
        if (details.Authors.Any())
        {
            foreach (var author in details.Authors)
            {
                Console.WriteLine("  - {0} (ID: {1})", author.FullName, author.Id);
            }
        }   
        else
        {
            Console.WriteLine("  No authors found");
        }
    
        Console.WriteLine("\nSubjects:");
        if (details.Subjects.Any())
        {
            foreach (var subject in details.Subjects)
            {
                Console.WriteLine("  - {0} (ID: {1})", subject.SubjectName, subject.Id);
            }
        }
        else
        {
            Console.WriteLine("  No subjects found");
        }
    }
    else
    {
        Console.WriteLine("Book not found with ISBN: 9780131103627");
    }
}

/* crud goals

=== Reading List App ===
1. Browse Books
2. My Reading List
3. Add Book to List
4. Update Reading Status  
5. Reading Goals
6. Reading Statistics
7. Search Books
8. Exit

*/


/* tui stuff
Application.Init();

try
{
    ExampleWindow exampleWindow = new ExampleWindow();
    exampleWindow.SetupApplication();

    Application.Run();
}
finally
{
    Application.Shutdown();
}
*/