using ReadingList.Services;
using ReadingList.Tui.Views;
using Terminal.Gui;

namespace ReadingList.Tui;

public class TuiApplication(IBookService bookService, IReadingListService readingListService, IReadingGoalService readingGoalService)
{
    private readonly IBookService _bookService = bookService;
    private readonly IReadingListService _readingListService = readingListService;
    private readonly IReadingGoalService _ReadingGoalService = readingGoalService;

    public void Run()
    {
        Application.Init();
        Toplevel top = Application.Top;

        MenuBar menu = CreateTopMenu();
        StatusBar statusBar = CreateStatusBar();

        top.Add(menu, statusBar);

        var mainView = new MainView(_bookService, _readingListService, _ReadingGoalService);
        top.Add(mainView);

        Application.Run();
        Application.Shutdown();
    }

    private MenuBar CreateTopMenu()
    {
        return new MenuBar(new MenuBarItem[]
        {
            new MenuBarItem("_File", new MenuItem[]
            {
                new MenuItem("_Quit", "", () => Application.RequestStop())
            })
        });
    }

    private StatusBar CreateStatusBar()
    {
        return new StatusBar(new StatusItem[]
        {
            new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => Application.RequestStop())
        });
    }
}