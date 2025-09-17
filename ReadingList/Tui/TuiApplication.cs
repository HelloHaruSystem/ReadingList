using ReadingList.Tui.Views;
using Terminal.Gui;

namespace ReadingList.Tui;

public class TuiApplication
{
    private readonly MainView _mainView;

    public TuiApplication(MainView mainView)
    {
        _mainView = mainView;
    }

    public void Run()
    {
        Application.Init();
        Toplevel top = Application.Top;

        // Apply a built-in color scheme to the entire TUI
        top.ColorScheme = Colors.Base;

        MenuBar menu = CreateTopMenu();
        StatusBar statusBar = CreateStatusBar();

        top.Add(menu, statusBar);
        top.Add(_mainView);

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