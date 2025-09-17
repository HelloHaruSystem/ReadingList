using ReadingList.Services;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class MainView : Window
{
    private readonly IBookService _bookService;
    private readonly IReadingListService _readingListService;
    private readonly IReadingGoalService _readingGoalService;

    public MainView(IBookService bookService, IReadingListService readingListService, IReadingGoalService readingGoalService)
    {
        _bookService = bookService;
        _readingListService = readingListService;
        _readingGoalService = readingGoalService;

        SetupUI();
    }

    private void SetupUI()
    {
        // window properties
        Title = "Reading List App";
        X = 0;
        Y = 1;
        Width = Dim.Fill();
        Height = Dim.Fill() - 1;

        // create main menu and add it to the application
        var menu = CreateMenuListView();
        Add(menu);
    }

    private ListView CreateMenuListView()
    {
        string[] menuOptions = new string[]
        {
            "1. Browse Books",
            "2. My Reading List",
            "3. Add Book to List",
            "4. Update Reading Status",
            "5. Reading Goals",
            "6. Reading Statistics",
            "7. Search Books",
            "8. Exit"
        };

        ListView menuList = new ListView(menuOptions)
        {
            X = Pos.Center(),
            Y = Pos.Center(),

            Width = 25,
            Height = menuOptions.Length
        };

        // handle menu selection
        menuList.OpenSelectedItem += async (args) =>
        {
            switch (args.Item)
            {
                case 6: // search books
                    string searchTerm = "programming";
                    var searchResults = await _bookService.SearchBooksAsync(searchTerm);
                    var resultText = $"Found {searchResults.Count()} books matching '{searchTerm}'!";
                    MessageBox.Query(50, 7, "Search Results", resultText, "OK");
                    break;
                case 7: // Exit
                    Application.RequestStop();
                    break;
                default:
                    MessageBox.Query(50, 7, "Action", $"{menuOptions[args.Item]} is not yet implemented.", "OK");
                    break;
            }
        };

        return menuList;
    }
}