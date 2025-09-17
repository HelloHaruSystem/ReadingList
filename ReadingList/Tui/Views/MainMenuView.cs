using ReadingList.Tui.Configuration;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class MainMenuView : BaseView
{
    public MainMenuView(NavigationManager navigationmanager)
        : base("Reading List - Main Menu", navigationmanager)
    {
        SetNavigationManager(_navigationManager);
        SetupUI();
    }

    protected override void SetupUI()
    {
        FrameView menuFrame = new FrameView("Main Menu")
        {
            X = Pos.Center() - UiConstants.Layout.MainMenuCenterOffset,
            Y = Pos.Center() - 5,
            Width = UiConstants.Layout.MainMenuWidth,
            Height = UiConstants.Layout.MainMenuHeight
        };

        ListView menuList = CreateMainMenu();
        menuFrame.Add(menuList);
        Add(menuFrame);
    }

    private ListView CreateMainMenu()
    {
        string[] menuOptions = new string[]
        {
            "1. Browse All Books",
            "2. Search Books", 
            "3. My Reading List",
            "4. Add Book to List",
            "5. Update Reading Status",
            "6. Rate a Book",
            "7. Reading Goals",
            "8. Reading Statistics",
            "9. Exit"
        };

        ListView menuList = new ListView(menuOptions)
        {
            X = 0,
            Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill()
        };

        menuList.OpenSelectedItem += OnMenuItemSelected;

        return menuList;
    }

    private void OnMenuItemSelected(ListViewItemEventArgs args)
    {
        switch (args.Item)
        {
            case 0: // Browse All Books
                NavigateToBrowseBooksView();
                break;
            case 1: // Search Books
                NavigateToSearchBooksView();
                break;
            case 2: // My Reading List
                NavigateToMyReadingListView();
                break;
            case 3: // Add Book to List
                NavigateToAddBookView();
                break;
            case 4: // Update Reading Status
                NavigateToUpdateStatusView();
                break;
            case 5: // Rate a Book
                NavigateToRateBookView();
                break;
            case 6: // Reading Goals
                NavigateToReadingGoalsView();
                break;
            case 7: // Reading Statistics  
                NavigateToStatisticsView();
                break;
            case 8: // Exit
                Application.RequestStop();
                break;
        }
    }

    // Navigation methods
    private void NavigateToBrowseBooksView()
    {
        BrowseBooksView browseBooksView = _navigationManager.GetView<BrowseBooksView>();
        _navigationManager.NavigateTo(browseBooksView);
    }

    private void NavigateToSearchBooksView()
    {
        SearchBooksView searchBooksView = _navigationManager.GetView<SearchBooksView>();
        _navigationManager.NavigateTo(searchBooksView);
    }

    private void NavigateToMyReadingListView()
    {
        MyReadingListView myReadingListView = _navigationManager.GetView<MyReadingListView>();
        _navigationManager.NavigateTo(myReadingListView);
    }

    private void NavigateToAddBookView()
    {
        AddBookView addBookView = _navigationManager.GetView<AddBookView>();
        _navigationManager.NavigateTo(addBookView);
    }

    private void NavigateToUpdateStatusView()
    {
        UpdateStatusView updateStatusView = _navigationManager.GetView<UpdateStatusView>();
        _navigationManager.NavigateTo(updateStatusView);
    }

    private void NavigateToRateBookView()
    {
        RateBookView rateBookView = _navigationManager.GetView<RateBookView>();
        _navigationManager.NavigateTo(rateBookView);
    }

    private void NavigateToReadingGoalsView()
    {
        ReadingGoalsView readingGoalsView = _navigationManager.GetView<ReadingGoalsView>();
        _navigationManager.NavigateTo(readingGoalsView);
    }

    private void NavigateToStatisticsView()
    {
        StatisticsView statisticsView = _navigationManager.GetView<StatisticsView>();
        _navigationManager.NavigateTo(statisticsView);
    }
}