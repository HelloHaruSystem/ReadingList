using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadingList.Data;
using ReadingList.Services;
using ReadingList.Tui;
using ReadingList.Tui.Views;

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

// confg
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// setup dependenciy incjetion container
IServiceCollection services = new ServiceCollection();

// register config
services.AddSingleton<IConfiguration>(config);

// register repositories
services.AddScoped<IBookRepository, BookRepository>();
services.AddScoped<IUserBookRepository, UserBookRepository>();
services.AddScoped<IReadingGoalRepository, ReadingGoalRepository>();

// register services
services.AddScoped<IBookService, BookService>();
services.AddScoped<IReadingListService, ReadingListService>();
services.AddScoped<IReadingGoalService, ReadingGoalService>();

// register Views
services.AddScoped<MainView>();

// register tui
services.AddScoped<TuiApplication>();

// builder service
ServiceProvider serviceProvider = services.BuildServiceProvider();

// create and start the tui app
TuiApplication app = serviceProvider.GetService<TuiApplication>() 
    ?? throw new Exception("Failed to create TuiApplication");

// Run the App
app.Run();

// clean up
serviceProvider.Dispose();