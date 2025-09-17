using ReadingList.Models;
using ReadingList.Models.Enums;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class MyReadingListView : BaseView
{
    private readonly IReadingListService _readingListService;
    private readonly ListView _booksListView = new ListView();
    private readonly ComboBox _statusFilterCombo = new ComboBox();
    private List<UserBook> _userBooks;
    private ReadingStatus? _currentFilter;

    public MyReadingListView(
        NavigationManager navigationManager,
        IReadingListService readingListService) 
        : base("My Reading List", navigationManager)
    {
        _readingListService = readingListService;
        _userBooks = new List<UserBook>();
        _currentFilter = null;

        SetupUI();
    }

    protected override void SetupUI()
    {
        // Filter section in a frame
        FrameView filterFrame = new FrameView("Filter")
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = 4
        };

        Label filterLabel = new Label("By Status:")
        {
            X = 1,
            Y = 0
        };

        _statusFilterCombo.X = Pos.Right(filterLabel) + 2;
        _statusFilterCombo.Y = 0;
        _statusFilterCombo.Width = 20;
        _statusFilterCombo.Height = 8;

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

        filterFrame.Add(filterLabel, _statusFilterCombo);

        // Books list in a frame
        FrameView booksFrame = new FrameView("My Books")
        {
            X = 1,
            Y = 5,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 10
        };

        _booksListView.X = 1;
        _booksListView.Y = 1;
        _booksListView.Width = Dim.Fill() - 2;
        _booksListView.Height = Dim.Fill() - 2;

        _booksListView.OpenSelectedItem += OnBookSelected;
        booksFrame.Add(_booksListView);

        // Action buttons in a frame
        FrameView actionFrame = new FrameView("Actions")
        {
            X = 1,
            Y = Pos.Bottom(booksFrame),
            Width = Dim.Fill() - 2,
            Height = 4
        };

        Button backButton = new Button("Back")
        {
            X = 1,
            Y = 1
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        Button refreshButton = new Button("Refresh")
        {
            X = Pos.Right(backButton) + 2,
            Y = 1
        };

        refreshButton.Clicked += async () => await LoadBooksAsync();

        actionFrame.Add(backButton, refreshButton);
        Add(filterFrame, booksFrame, actionFrame);
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
                        $"Personal Rating: {rating}/5\n\n" +
                        $"Date Started: {userBook.DateStarted:yyyy-MM-dd}\n\n" +
                        $"Last Updated: {userBook.UpdatedAt:yyyy-MM-dd}\n\n" +
                        $"Personal Notes:\n{notes}\n\n" +
                        $"Description: {userBook.Book.Description ?? "No description available."}";

        MessageBox.Query(80, 18, "Book Details", details, "OK");
    }
}