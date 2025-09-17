using Terminal.Gui;

namespace ReadingList.Tui.Views.Base;

public abstract class BaseView : Window
{
    protected BaseView(string title) : base(title)
    {
        X = 0;
        Y = 1;                   // space for the menu bar
        Width = Dim.Fill();
        Height = Dim.Fill() - 1; // space for the status bar
    }

    protected virtual void SetupUI()
    {
        // Override in derived classes
    }

    public virtual void OnViewActivated()
    {
        // Called when view becomes active - override for refresh logic
    }

    protected void ShowMessage(string title, string message)
    {
        MessageBox.Query(60, 8, title, message, "OK");
    }

    protected void ShowError(string message)
    {
        MessageBox.ErrorQuery(60, 8, "Error", message, "OK");
    }

    protected bool ShowConfirmation(string title, string message)
    {
        return MessageBox.Query(60, 8, title, message, "Yes", "No") == 0;
    }
}