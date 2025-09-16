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
        System.Console.Write("{0} ({1})\n", b.Title, b.PublicationYear);
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