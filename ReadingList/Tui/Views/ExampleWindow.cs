using Terminal.Gui;

namespace ReadingList.Tui.Views;

[Obsolete]
public class ExampleWindow : Window
{
    public ExampleWindow() : base("Example tui window")
    {
        X = 0;
        Y = 1;
        Width = Dim.Fill();
        Height = Dim.Fill() - 1;

        InitializeComponent();
    }

    private void InitializeComponent()
    {
        // Create some controls
        var hello = new Label("Hello World! Press ESC to quit.")
        {
            X = Pos.Center(),
            Y = Pos.Center()
        };

        var button = new Button("Click Me!")
        {
            X = Pos.Center(),
            Y = Pos.Center() + 2
        };

        // Handle button click
        button.Clicked += () =>
        {
            MessageBox.Query(50, 7, "Information", "Button was clicked!", "OK");
        };

        // Add controls to this window
        Add(hello, button);
    }

    public void SetupApplication()
    {
        var top = Application.Top;

        // Create a menu bar
        var menu = new MenuBar(new MenuBarItem[] {
            new MenuBarItem("_File", new MenuItem?[] {
                new MenuItem("_New", "Creates new file", null),
                new MenuItem("_Open", "Opens a file", null),
                null, // separator
                new MenuItem("_Quit", "Exits the application", () => Application.RequestStop())
            }),
            new MenuBarItem("_Edit", new MenuItem[] {
                new MenuItem("_Copy", "Copy text", null),
                new MenuItem("_Paste", "Paste text", null)
            })
        });

        // Create a status bar
        var statusBar = new StatusBar(new StatusItem[] {
            new StatusItem(Key.F1, "~F1~ Help", null),
            new StatusItem(Key.F2, "~F2~ Save", null),
            new StatusItem(Key.CtrlMask | Key.Q, "~^Q~ Quit", () => Application.RequestStop())
        });

        // Add everything to the top level
        top.Add(menu, this, statusBar);
    }
}