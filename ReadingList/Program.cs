using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ReadingList.Data;
using ReadingList.Services;
using ReadingList.Tui;
using ReadingList.Tui.Views;

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