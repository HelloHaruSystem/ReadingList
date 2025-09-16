using Microsoft.Extensions.Configuration;
using ReadingList.Data;
using ReadingList.Services;
using ReadingList.Tui;
using ReadingList.Tui.Views;
using Terminal.Gui;

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

// TODO: add invertion of controll container if I have time
// dependencies
// confg
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// repositories
IBookRepository bookRepo = new BookRepository(config);
IUserBookRepository userBookRepo = new UserBookRepository(config);
IReadingGoalRepository readingGoalRepo = new ReadingGoalRepository(config);

// services
IBookService bookService = new BookService(bookRepo);
IReadingListService readingListService = new ReadingListService(userBookRepo);
IReadingGoalService readingGoalService = new ReadingGoalService(readingGoalRepo);

// create and start tui app
TuiApplication tuiApp = new TuiApplication(
    bookService,
    readingListService,
    readingGoalService
);

// Run the App
tuiApp.Run();