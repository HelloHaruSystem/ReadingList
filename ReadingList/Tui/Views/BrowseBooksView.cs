using ReadingList.Models;
using ReadingList.Services;
using ReadingList.Tui.Configuration;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class BrowseBooksView : BaseView
{
    private readonly IBookService _bookService;
    private readonly ListView _booksListView = new ListView();
    private List<Book> _books = new List<Book>();

    public BrowseBooksView(NavigationManager navigationManager, IBookService bookService) 
        : base("Browse All Books", navigationManager)
    {
        _bookService = bookService;
        
        SetupUI();
    }

    protected override void SetupUI()
    {
        // init listView fields
        _booksListView.X = UiConstants.Frames.DefaultMargin;
        _booksListView.Y = UiConstants.Frames.DefaultMargin;
        _booksListView.Width = Dim.Fill() - (UiConstants.Frames.DefaultMargin * 2);
        _booksListView.Height = Dim.Fill() - UiConstants.Frames.SmallFrameHeight;
        _booksListView.OpenSelectedItem += OnBookSelected;

        // Create back button
        Button backButton = new Button("Back")
        {
            X = UiConstants.Frames.DefaultMargin,
            Y = Pos.Bottom(this) - UiConstants.Layout.BottomButtonOffset
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        // Create refresh button
        Button refreshButton = new Button("Refresh")
        {
            X = Pos.Right(backButton) + UiConstants.Frames.DefaultSpacing,
            Y = Pos.Bottom(this) - UiConstants.Layout.BottomButtonOffset
        };

        refreshButton.Clicked += async () => await LoadBooksAsync();

        Add(_booksListView, backButton, refreshButton);
    }

    public override async void OnViewActivated()
    {
        await LoadBooksAsync();
    }

    private async Task LoadBooksAsync()
    {
        try
        {
            IEnumerable<Book> books = await _bookService.GetAllBooksAsync();
            _books = books.ToList();

            // Convert books to display strings
            string[] bookDisplayItems = _books.Select(FormatBookForDisplay).ToArray();
            _booksListView.SetSource(bookDisplayItems);

            SetNeedsDisplay();
        }
        catch (Exception ex)
        {
            ShowError($"Failed to load books: {ex.Message}");
        }
    }

    private string FormatBookForDisplay(Book book)
    {
        string year = book.PublicationYear?.ToString() ?? "N/A";
        string pages = book.Pages?.ToString() ?? "N/A";
        return $"{book.Title} ({year}) - {pages} pages";
    }

    private async void OnBookSelected(ListViewItemEventArgs args)
    {
        if (args.Item >= 0 && args.Item < _books.Count)
        {
            Book selectedBook = _books[args.Item];
            await ShowBookDetailsAsync(selectedBook);
        }
    }

    private async Task ShowBookDetailsAsync(Book book)
    {
        try
        {
            // Get full book details with authors and subjects
            Book? detailedBook = await _bookService.GetBookWithDetailsAsync(book.ISBN);
            
            if (detailedBook == null)
            {
                ShowError("Could not load book details.");
                return;
            }

            string authors = detailedBook.Authors.Any() 
                ? string.Join(", ", detailedBook.Authors.Select(a => a.FullName))
                : "Unknown";

            string subjects = detailedBook.Subjects.Any()
                ? string.Join(", ", detailedBook.Subjects.Select(s => s.SubjectName))
                : "None";

            string details = $"Title: {detailedBook.Title}\n\n" +
                           $"Authors: {authors}\n\n" +
                           $"Subjects: {subjects}\n\n" +
                           $"Year: {detailedBook.PublicationYear?.ToString() ?? "N/A"}\n\n" +
                           $"Pages: {detailedBook.Pages?.ToString() ?? "N/A"}\n\n" +
                           $"ISBN: {detailedBook.ISBN}\n\n" +
                           $"Description: {detailedBook.Description ?? "No description available."}";

            MessageBox.Query(80, 20, "Book Details", details, "OK");
        }
        catch (Exception ex)
        {
            ShowError($"Error loading book details: {ex.Message}");
        }
    }
}