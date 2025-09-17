using ReadingList.Services;
using ReadingList.Tui.Views;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui;

public class TuiApplication
{
    private readonly IBookService _bookService;
    private readonly IReadingListService _readingListService;
    private readonly IReadingGoalService _readingGoalService;

    public TuiApplication(
        IBookService bookService,
        IReadingListService readingListService,
        IReadingGoalService readingGoalService)
    {
        _bookService = bookService;
        _readingListService = readingListService;
        _readingGoalService = readingGoalService;
    }

    public void Run()
    {
        Application.Init();
        Toplevel top = Application.Top;

        // Apply color scheme
        top.ColorScheme = Colors.Base;

        // Create navigation manager
        NavigationManager navigationManager = new NavigationManager(top);

        // Create main menu view
        MainMenuView mainMenuView = new MainMenuView(
            _bookService,
            _readingListService, 
            _readingGoalService,
            navigationManager);

        // Setup top-level UI
        MenuBar menu = CreateTopMenu();
        StatusBar statusBar = CreateStatusBar();

        top.Add(menu, statusBar);

        // Start navigation with main menu
        navigationManager.NavigateTo(mainMenuView);

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
            new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => Application.RequestStop()),
            new StatusItem(Key.Unknown, "~TAB~ Navigate  ~ESC~ Back", null)
        });
    }
}