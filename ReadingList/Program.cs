using Microsoft.Extensions.Configuration;
using ReadingList.Tui.Views;
using Terminal.Gui;

IConfigurationRoot config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();


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


// tui stuff
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
