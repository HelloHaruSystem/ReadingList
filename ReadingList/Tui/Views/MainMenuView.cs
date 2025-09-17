using Readinglist.Tui.Views;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class MainMenuView : BaseView
{
    private readonly IBookService _bookService;
    private readonly IReadingListService _readingListService;
    private readonly IReadingGoalService _readingGoalService;
    private readonly NavigationManager _navigationManager;

    public MainMenuView(
        IBookService bookService,
        IReadingListService readingListService,
        IReadingGoalService readingGoalService,
        NavigationManager navigationManager) 
        : base("Reading List - Main Menu")
    {
        _bookService = bookService;
        _readingListService = readingListService;
        _readingGoalService = readingGoalService;
        _navigationManager = navigationManager;

        SetNavigationManager(navigationManager);
        SetupUI();
    }

    protected override void SetupUI()
    {
        FrameView menuFrame = new FrameView("Main Menu")
        {
            X = Pos.Center() - 18,
            Y = Pos.Center() - 5,
            Width = 36,
            Height = 12
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
        BrowseBooksView browseBooksView = new BrowseBooksView(_bookService, _navigationManager);
        _navigationManager.NavigateTo(browseBooksView);
    }

    private void NavigateToSearchBooksView()
    {
        SearchBooksView searchBooksView = new SearchBooksView(_bookService, _navigationManager);
        _navigationManager.NavigateTo(searchBooksView);
    }

    private void NavigateToMyReadingListView()
    {
        MyReadingListView myReadingListView = new MyReadingListView(_readingListService, _navigationManager);
        _navigationManager.NavigateTo(myReadingListView);
    }

    private void NavigateToAddBookView()
    {
        AddBookView addBookView = new AddBookView(_bookService, _readingListService, _navigationManager);
        _navigationManager.NavigateTo(addBookView);
    }

    private void NavigateToUpdateStatusView()
    {
        UpdateStatusView updateStatusView = new UpdateStatusView(_readingListService, _navigationManager);
        _navigationManager.NavigateTo(updateStatusView);
    }

    private void NavigateToRateBookView()
    {
        RateBookView rateBookView = new RateBookView(_readingListService, _navigationManager);
        _navigationManager.NavigateTo(rateBookView);
    }

    private void NavigateToReadingGoalsView()
    {
        ShowMessage("Info", "Reading Goals view - Coming next!");
    }

    private void NavigateToStatisticsView()
    {
        ShowMessage("Info", "Statistics view - Coming next!");
    }
}