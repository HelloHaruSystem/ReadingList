using Microsoft.Extensions.DependencyInjection;
using Terminal.Gui;

namespace ReadingList.Tui.Views.Base;

public class NavigationManager
{
    private readonly Stack<BaseView> _viewStack = new();
    private readonly Toplevel _top;
    private readonly IServiceProvider _serviceProvider;

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
            _top.Remove(currentView);
        }

        // Add view
        _viewStack.Push(view);
        _top.Add(view);

        // Activate view
        view.OnViewActivated();

        // Set focus refresh
        view.SetFocus();
        _top.SetNeedsDisplay();
    }
    
    public void NavigateBack()
    {
        if (_viewStack.Count <= 1) return; // Don't remove the main view

        // Remove current view
        BaseView currentView = _viewStack.Pop();
        _top.Remove(currentView);

        // Show previous view
        if (_viewStack.Count > 0)
        {
            BaseView previousView = _viewStack.Peek();
            _top.Add(previousView);
            previousView.OnViewActivated();
            previousView.SetFocus();
            _top.SetNeedsDisplay();
        }
    }

    public void NavigateToMain(BaseView mainView)
    {
        // Clear all views and go to main
        while (_viewStack.Count > 0)
        {
            var view = _viewStack.Pop();
            _top.Remove(view);
        }

        NavigateTo(mainView);
    }

    public bool CanNavigateBack => _viewStack.Count > 1;
}