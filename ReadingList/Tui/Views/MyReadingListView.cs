using ReadingList.Models;
using ReadingList.Models.Enums;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace Readinglist.Tui.Views;

public class MyReadingListView : BaseView
{
    private readonly IReadingListService _readingListService;
    private readonly NavigationManager _navigationManager;
    private ListView _booksListView;
    private ComboBox _statusFilterCombo;
    private List<UserBook> _userBooks;
    private ReadingStatus? _currentFilter;

    public MyReadingListView(
        IReadingListService readingListService,
        NavigationManager navigationManager) 
        : base("My Reading List")
    {
        _readingListService = readingListService;
        _navigationManager = navigationManager;
        _userBooks = new List<UserBook>();
        _currentFilter = null;

        SetNavigationManager(navigationManager);
        SetupUI();
    }

    protected override void SetupUI()
    {
        // Status filter label and combo
        Label filterLabel = new Label("Filter by Status:")
        {
            X = 1,
            Y = 1
        };

        _statusFilterCombo = new ComboBox()
        {
            X = Pos.Right(filterLabel) + 2,
            Y = 1,
            Width = 20,
            Height = 8
        };

        _statusFilterCombo.SetSource(new string[] 
        { 
            "All", 
            "To Read", 
            "Currently Reading", 
            "Completed", 
            "Paused", 
            "Abandoned" 
        });

        _statusFilterCombo.Text = "All";
        _statusFilterCombo.SelectedItemChanged += OnStatusFilterChanged;

        // Books list view
        _booksListView = new ListView()
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 6
        };

        _booksListView.OpenSelectedItem += OnBookSelected;

        // Buttons
        Button backButton = new Button("Back")
        {
            X = 1,
            Y = Pos.Bottom(this) - 3
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        Button refreshButton = new Button("Refresh")
        {
            X = Pos.Right(backButton) + 2,
            Y = Pos.Bottom(this) - 3
        };

        refreshButton.Clicked += async () => await LoadBooksAsync();

        Add(filterLabel, _statusFilterCombo, _booksListView, backButton, refreshButton);
    }

    public override async void OnViewActivated()
    {
        await LoadBooksAsync();
    }

    private async Task LoadBooksAsync()
    {
        try
        {
            IEnumerable<UserBook> books;

            if (_currentFilter == null)
            {
                books = await _readingListService.GetMyReadingListAsync();
            }
            else
            {
                books = await _readingListService.GetBooksByStatusAsync(_currentFilter.Value);
            }

            _userBooks = books.ToList();

            // Convert to display strings
            string[] bookDisplayItems = _userBooks.Select(FormatUserBookForDisplay).ToArray();
            _booksListView.SetSource(bookDisplayItems);

            SetNeedsDisplay();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load reading list: {ex.Message}");
        }
    }

    private string FormatUserBookForDisplay(UserBook userBook)
    {
        string status = GetStatusDisplayName(userBook.Status);
        string rating = userBook.PersonalRating?.ToString() ?? "Not rated";
        string year = userBook.Book.PublicationYear?.ToString() ?? "N/A";
        
        return $"{userBook.Book.Title} ({year}) - {status} - Rating: {rating}";
    }

    private string GetStatusDisplayName(ReadingStatus status)
    {
        return status switch
        {
            ReadingStatus.ToRead => "To Read",
            ReadingStatus.CurrentlyReading => "Currently Reading",
            ReadingStatus.Completed => "Completed",
            ReadingStatus.Paused => "Paused",
            ReadingStatus.Abandoned => "Abandoned",
            _ => "Unknown"
        };
    }

    private async void OnStatusFilterChanged(ListViewItemEventArgs args)
    {
        _currentFilter = args.Item switch
        {
            0 => null, // All
            1 => ReadingStatus.ToRead,
            2 => ReadingStatus.CurrentlyReading,
            3 => ReadingStatus.Completed,
            4 => ReadingStatus.Paused,
            5 => ReadingStatus.Abandoned,
            _ => null
        };

        await LoadBooksAsync();
    }

    private void OnBookSelected(ListViewItemEventArgs args)
    {
        if (args.Item >= 0 && args.Item < _userBooks.Count)
        {
            UserBook selectedBook = _userBooks[args.Item];
            ShowUserBookDetails(selectedBook);
        }
    }

    private void ShowUserBookDetails(UserBook userBook)
    {
        string status = GetStatusDisplayName(userBook.Status);
        string rating = userBook.PersonalRating?.ToString() ?? "Not rated";
        string notes = !string.IsNullOrWhiteSpace(userBook.PersonalNotes) 
            ? userBook.PersonalNotes 
            : "No notes";

        string details = $"Title: {userBook.Book.Title}\n\n" +
                        $"Status: {status}\n\n" +
                        $"Personal Rating: {rating}/10\n\n" +
                        $"Date Started: {userBook.DateStarted:yyyy-MM-dd}\n\n" +
                        $"Last Updated: {userBook.UpdatedAt:yyyy-MM-dd}\n\n" +
                        $"Personal Notes:\n{notes}\n\n" +
                        $"Description: {userBook.Book.Description ?? "No description available."}";

        MessageBox.Query(80, 18, "Book Details", details, "OK");
    }
}