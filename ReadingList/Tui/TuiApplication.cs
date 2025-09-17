using ReadingList.Services;
using ReadingList.Tui.Views;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui;

public class TuiApplication
{
    private readonly NavigationManager _navigationManager;

    public TuiApplication(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public void Run()
    {
        Application.Init();
        Toplevel top = Application.Top;

        // Apply color scheme
        top.ColorScheme = Colors.Base;

        // Setup top-level UI
        MenuBar menu = CreateTopMenu();
        StatusBar statusBar = CreateStatusBar();
        top.Add(menu, statusBar);

        MainMenuView mainMenuView = _navigationManager.GetView<MainMenuView>();
        _navigationManager.NavigateTo(mainMenuView);

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