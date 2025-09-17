using ReadingList.Tui.Configuration;
using Terminal.Gui;

namespace ReadingList.Tui.Views.Base;

public abstract class BaseView : Window
{
    protected NavigationManager _navigationManager;

    protected BaseView(string title, NavigationManager navigationManager) : base(title)
    {
        _navigationManager = navigationManager
            ?? throw new ArgumentNullException(nameof(navigationManager));

        X = 0;
        Y = UiConstants.Layout.MenuBarHeight;
        Width = Dim.Fill();
        Height = Dim.Fill() - UiConstants.Layout.MenuBarHeight;
        
        // Handle ESC key globally for all views
        KeyPress += OnKeyPress;
    }

    protected void SetNavigationManager(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    private void OnKeyPress(KeyEventEventArgs args)
    {
        if (args.KeyEvent.Key == Key.Esc && _navigationManager != null && _navigationManager.CanNavigateBack)
        {
            _navigationManager.NavigateBack();
            args.Handled = true;
        }
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