using ReadingList.Views;
using Terminal.Gui;

Console.WriteLine("Hello, World!");

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