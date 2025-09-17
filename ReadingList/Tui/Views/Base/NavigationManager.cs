using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace ReadingList.Tui.Views.Base;

public class NavigationManager
{
    private readonly Stack<BaseView> _viewStack = new();
    private readonly IServiceProvider _serviceProvider;
    private Toplevel? _top;

    // Property that lazily gets the Top when first accessed
    private Toplevel Top => _top ??= Application.Top
        ?? throw new InvalidOperationException("Terminal.Gui Application not initialized");

    public NavigationManager(IServiceProvider serviceProvider)
    {
        _top = Application.Top;
        _serviceProvider = serviceProvider;
    }

    public T GetView<T>() where T : BaseView
    {
        return _serviceProvider.GetRequiredService<T>();
    }

    public void NavigateTo(BaseView view)
    {
        // Remove current view if exists
        if (_viewStack.Count > 0)
        {
            BaseView currentView = _viewStack.Peek();
            Top.Remove(currentView);
        }

        // Add view
        _viewStack.Push(view);
        Top.Add(view);

        // Activate view
        view.OnViewActivated();

        // Set focus refresh
        view.SetFocus();
        Top.SetNeedsDisplay();
    }
    
    public void NavigateBack()
    {
        if (_viewStack.Count <= 1) return; // Don't remove the main view

        // Remove current view
        BaseView currentView = _viewStack.Pop();
        Top.Remove(currentView);

        // Show previous view
        if (_viewStack.Count > 0)
        {
            BaseView previousView = _viewStack.Peek();
            Top.Add(previousView);
            previousView.OnViewActivated();
            previousView.SetFocus();
            Top.SetNeedsDisplay();
        }
    }

    public void NavigateToMain(BaseView mainView)
    {
        // Clear all views and go to main
        while (_viewStack.Count > 0)
        {
            var view = _viewStack.Pop();
            Top.Remove(view);
        }

        NavigateTo(mainView);
    }

    public bool CanNavigateBack => _viewStack.Count > 1;
}