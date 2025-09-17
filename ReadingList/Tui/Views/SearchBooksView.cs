using ReadingList.Models;
using ReadingList.Services;
using ReadingList.Tui.Views.Base;
using Terminal.Gui;

namespace ReadingList.Tui.Views;

public class SearchBooksView : BaseView
{
    private readonly IBookService _bookService;
    private readonly NavigationManager _navigationManager;
    private TextField _searchTextField;
    private ListView _resultsListView;
    private Label _resultsLabel;
    private List<Book> _searchResults;

    public SearchBooksView(
        IBookService bookService,
        NavigationManager navigationManager) 
        : base("Search Books")
    {
        _bookService = bookService;
        _navigationManager = navigationManager;
        _searchResults = new List<Book>();

        SetNavigationManager(navigationManager);
        SetupUI();
    }

    protected override void SetupUI()
    {
        // Search input section
        Label searchLabel = new Label("Search Term:")
        {
            X = 1,
            Y = 1
        };

        _searchTextField = new TextField()
        {
            X = Pos.Right(searchLabel) + 2,
            Y = 1,
            Width = 30
        };

        Button searchButton = new Button("Search")
        {
            X = Pos.Right(_searchTextField) + 2,
            Y = 1
        };

        searchButton.Clicked += async () => await PerformSearchAsync();

        // Handle Enter key in text field
        _searchTextField.KeyPress += (args) =>
        {
            if (args.KeyEvent.Key == Key.Enter)
            {
                Task.Run(PerformSearchAsync);
                args.Handled = true;
            }
        };

        // Results section
        _resultsLabel = new Label("Enter search term and press Search")
        {
            X = 1,
            Y = 3
        };

        _resultsListView = new ListView()
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill() - 2,
            Height = Dim.Fill() - 7
        };

        _resultsListView.OpenSelectedItem += OnBookSelected;

        // Buttons
        Button backButton = new Button("Back")
        {
            X = 1,
            Y = Pos.Bottom(this) - 3
        };

        backButton.Clicked += () => _navigationManager.NavigateBack();

        Button clearButton = new Button("Clear")
        {
            X = Pos.Right(backButton) + 2,
            Y = Pos.Bottom(this) - 3
        };

        clearButton.Clicked += () => ClearSearch();

        Add(searchLabel, _searchTextField, searchButton, _resultsLabel, _resultsListView, backButton, clearButton);
    }

    public override void OnViewActivated()
    {
        // Set focus to search field when view is activated
        _searchTextField.SetFocus();
    }

    private async Task PerformSearchAsync()
    {
        string searchTerm = _searchTextField.Text?.ToString()?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            _resultsLabel.Text = "Please enter a search term";
            _searchResults.Clear();
            _resultsListView.SetSource(new string[0]);
            SetNeedsDisplay();
            return;
        }

        try
        {
            _resultsLabel.Text = "Searching...";
            SetNeedsDisplay();

            IEnumerable<Book> results = await _bookService.SearchBooksAsync(searchTerm);
            _searchResults = results?.ToList() ?? new List<Book>();

            UpdateResultsDisplay(searchTerm);
        }
        catch (Exception ex)
        {
            _resultsLabel.Text = "Search failed";
            _searchResults.Clear();
            _resultsListView.SetSource(new string[0]);
            SetNeedsDisplay();
            
            // Log the actual error for debugging
            Console.WriteLine($"Search error: {ex.Message}");
        }
    }

    private void UpdateResultsDisplay(string searchTerm)
    {
        if (_searchResults.Count == 0)
        {
            _resultsLabel.Text = $"No books found for '{searchTerm}'";
            _resultsListView.SetSource(new string[0]);
        }
        else
        {
            _resultsLabel.Text = $"Found {_searchResults.Count} books for '{searchTerm}'";
            string[] bookDisplayItems = _searchResults.Select(FormatBookForDisplay).ToArray();
            _resultsListView.SetSource(bookDisplayItems);
        }

        SetNeedsDisplay();
    }

    private string FormatBookForDisplay(Book book)
    {
        string year = book.PublicationYear?.ToString() ?? "N/A";
        string pages = book.Pages?.ToString() ?? "N/A";
        return $"{book.Title} ({year}) - {pages} pages";
    }

    private void ClearSearch()
    {
        _searchTextField.Text = "";
        _resultsLabel.Text = "Enter search term and press Search";
        _searchResults.Clear();
        _resultsListView.SetSource(new string[0]);
        _searchTextField.SetFocus();
        SetNeedsDisplay();
    }

    private async void OnBookSelected(ListViewItemEventArgs args)
    {
        if (args.Item >= 0 && args.Item < _searchResults.Count)
        {
            Book selectedBook = _searchResults[args.Item];
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