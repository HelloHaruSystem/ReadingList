using ReadingList.Models;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class BrowseBooksView : BaseView
{
    private readonly IBookService _bookService;
    private readonly NavigationManager _navigationManager;
    private ListView _booksListView;
    private List<Book> _books;

    public BrowseBooksView(
        IBookService bookService,
        NavigationManager navigationManager)
        : base("Browse All Books")
    {
        _bookService = bookService;
        _navigationManager = navigationManager;
        _books = new List<Book>();

        SetupUI();
    }

    protected override void SetupUI()
    {
        _booksListView = new ListView
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 4
        };

        _booksListView.OpenSelectedItem += OnBookSelected;

        Button backButton = new Button("Back")
        {
            X = 1,
            Y = Pos.Bottom(this) - 3
        };

        Button refreshButton = new Button("Refresh")
        {
            X = Pos.Right(backButton) + 2,
            Y = Pos.Bottom(this) - 3
        };

        // add components
        Add(_booksListView, backButton, refreshButton);

        // add events
        backButton.Clicked += () => _navigationManager.NavigateBack();
        refreshButton.Clicked += async () => await LoadBooksAsync();
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

            // Convert books to displayable strings
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