using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Readinglist.Tui.Views;
using ReadingList.Data;
using ReadingList.Services;
using ReadingList.Tui;
using ReadingList.Tui.Views;
using ReadingList.Tui.Views.Base;

// config
IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

// setup dependency injection container
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

// register navigation manager
services.AddSingleton<NavigationManager>();

// regiister views
services.AddScoped<MainMenuView>();
services.AddScoped<BrowseBooksView>();
services.AddScoped<SearchBooksView>();
services.AddScoped<MyReadingListView>();
services.AddScoped<AddBookView>();
services.AddScoped<UpdateStatusView>();
services.AddScoped<RateBookView>();
services.AddScoped<ReadingGoalsView>();
services.AddScoped<StatisticsView>();

// register tui application
services.AddScoped<TuiApplication>();

// build service provider
ServiceProvider serviceProvider = services.BuildServiceProvider();

// create and start the tui app
TuiApplication app = serviceProvider.GetService<TuiApplication>() 
    ?? throw new Exception("Failed to create TuiApplication");

// run the app
app.Run();

// clean up
serviceProvider.Dispose();